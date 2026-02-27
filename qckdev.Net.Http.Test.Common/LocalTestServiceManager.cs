using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace qckdev.Net.Http.Test.Common
{
    public static class LocalTestServiceManager
    {
        private static readonly object SyncLock = new object();
        private static InProcessTestService _service;
        private static bool _initialized;
        private static bool _ownedService;

        private const string DisableAutoStartEnvVar = "QCKDEV_NET_HTTP_TESTSERVICE_AUTO_START";

        public static Configuration.Settings NormalizeSettingsForCurrentFramework(Configuration.Settings settings)
        {
            if (settings == null)
            {
                return settings;
            }

            if (!HasLocalHostTarget(settings))
            {
                return settings;
            }

            int port = GetPortForCurrentProcess();
            settings.PokemonUrl = RewriteLocalUrlPort(settings.PokemonUrl, port);
            settings.JiraUrl = RewriteLocalUrlPort(settings.JiraUrl, port);
            settings.GorestUrl = RewriteLocalUrlPort(settings.GorestUrl, port);
            settings.MockbinUrl = RewriteLocalUrlPort(settings.MockbinUrl, port);

            return settings;
        }

        public static void StartIfNeeded(Configuration.Settings settings)
        {
            if (!ShouldUseLocalService(settings) || IsAutoStartDisabled())
            {
                return;
            }

            lock (SyncLock)
            {
                if (_initialized)
                {
                    return;
                }
                _initialized = true;

                Uri serviceUri = GetServiceBaseUri(settings);
                if (serviceUri == null)
                {
                    return;
                }

                if (IsServiceReachable(serviceUri))
                {
                    _ownedService = false;
                    return;
                }

                try
                {
                    _service = new InProcessTestService(serviceUri);
                    _service.Start();

                    if (!WaitForService(serviceUri, 15000))
                    {
                        StopCurrentService();
                        throw new InvalidOperationException(
                            string.Format("Local test service did not become ready at {0}.", serviceUri.GetLeftPart(UriPartial.Authority)));
                    }

                    _ownedService = true;
                    AppDomain.CurrentDomain.ProcessExit += (_, __) => StopIfOwned();
                    AppDomain.CurrentDomain.DomainUnload += (_, __) => StopIfOwned();
                }
                catch (HttpListenerException)
                {
                    // Another process likely started it first.
                    if (WaitForService(serviceUri, 5000))
                    {
                        _ownedService = false;
                        return;
                    }

                    StopCurrentService();
                    throw;
                }
                catch
                {
                    StopCurrentService();
                    throw;
                }
            }
        }

        public static void StopIfOwned()
        {
            lock (SyncLock)
            {
                if (!_ownedService)
                {
                    return;
                }

                StopCurrentService();
                _ownedService = false;
                _initialized = false;
            }
        }

        private static void StopCurrentService()
        {
            try
            {
                if (_service != null)
                {
                    _service.Stop();
                    _service.Dispose();
                }
            }
            catch
            {
                // Ignore shutdown failures in test teardown.
            }
            finally
            {
                _service = null;
            }
        }

        private static bool IsAutoStartDisabled()
        {
            string autoStart = Environment.GetEnvironmentVariable(DisableAutoStartEnvVar);
            return string.Equals(autoStart, "0", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(autoStart, "false", StringComparison.OrdinalIgnoreCase);
        }

        private static bool ShouldUseLocalService(Configuration.Settings settings)
        {
            return settings != null && HasLocalHostTarget(settings);
        }

        private static bool HasLocalHostTarget(Configuration.Settings settings)
        {
            return IsLocalAddress(settings.PokemonUrl) ||
                   IsLocalAddress(settings.JiraUrl) ||
                   IsLocalAddress(settings.GorestUrl) ||
                   IsLocalAddress(settings.MockbinUrl);
        }

        private static bool IsLocalAddress(string value)
        {
            if (IsNullOrWhiteSpace(value))
            {
                return false;
            }

            Uri uri;
            if (Uri.TryCreate(value, UriKind.Absolute, out uri))
            {
                return string.Equals(uri.Host, "localhost", StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        private static bool WaitForService(Uri serviceUri, int timeoutMilliseconds)
        {
            DateTime startedAt = DateTime.UtcNow;
            while ((DateTime.UtcNow - startedAt).TotalMilliseconds < timeoutMilliseconds)
            {
                if (IsServiceReachable(serviceUri))
                {
                    return true;
                }

                Thread.Sleep(200);
            }

            return false;
        }

        private static bool IsServiceReachable(Uri serviceUri)
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(serviceUri);
                request.Method = "GET";
                request.Timeout = 1000;
                request.ReadWriteTimeout = 1000;

                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    int statusCode = (int)response.StatusCode;
                    return statusCode >= 200 && statusCode < 500;
                }
            }
            catch
            {
                return false;
            }
        }

        private static Uri GetServiceBaseUri(Configuration.Settings settings)
        {
            return GetLocalUri(settings.PokemonUrl) ??
                   GetLocalUri(settings.JiraUrl) ??
                   GetLocalUri(settings.GorestUrl) ??
                   GetLocalUri(settings.MockbinUrl);
        }

        private static Uri GetLocalUri(string value)
        {
            if (IsNullOrWhiteSpace(value))
            {
                return null;
            }

            Uri uri;
            if (Uri.TryCreate(value, UriKind.Absolute, out uri) &&
                string.Equals(uri.Host, "localhost", StringComparison.OrdinalIgnoreCase))
            {
                return new Uri(uri.GetLeftPart(UriPartial.Authority) + "/");
            }

            return null;
        }

        private static string RewriteLocalUrlPort(string value, int port)
        {
            if (IsNullOrWhiteSpace(value))
            {
                return value;
            }

            Uri uri;
            if (!Uri.TryCreate(value, UriKind.Absolute, out uri))
            {
                return value;
            }
            if (!string.Equals(uri.Host, "localhost", StringComparison.OrdinalIgnoreCase))
            {
                return value;
            }

            var builder = new UriBuilder(uri) { Port = port };
            return builder.Uri.ToString();
        }

        private static int GetPortForCurrentProcess()
        {
            // Keep it deterministic per process so settings and service startup align.
            // Also avoids collisions when multiple test hosts run in parallel.
            int pid = Process.GetCurrentProcess().Id;
            return 20000 + (pid % 10000);
        }

        private static bool IsNullOrWhiteSpace(string value)
        {
            return string.IsNullOrEmpty(value) || value.Trim().Length == 0;
        }

        private sealed class InProcessTestService : IDisposable
        {
            private readonly HttpListener _listener;
            private readonly object _usersLock = new object();
            private readonly Dictionary<int, GoUser> _users = new Dictionary<int, GoUser>();
            private int _lastUserId = 1000;
            private volatile bool _running;
            private Thread _listenerThread;

            public InProcessTestService(Uri serviceUri)
            {
                _listener = new HttpListener();
                _listener.Prefixes.Add(NormalizePrefix(serviceUri));
            }

            public void Start()
            {
                _listener.Start();
                _running = true;

                _listenerThread = new Thread(ListenLoop);
                _listenerThread.IsBackground = true;
                _listenerThread.Start();
            }

            public void Stop()
            {
                _running = false;

                try
                {
                    _listener.Stop();
                }
                catch
                {
                }

                try
                {
                    _listener.Close();
                }
                catch
                {
                }

                try
                {
                    if (_listenerThread != null && _listenerThread.IsAlive)
                    {
                        _listenerThread.Join(1000);
                    }
                }
                catch
                {
                }
            }

            public void Dispose()
            {
                Stop();
            }

            private void ListenLoop()
            {
                while (_running)
                {
                    HttpListenerContext context = null;

                    try
                    {
                        context = _listener.GetContext();
                    }
                    catch (HttpListenerException)
                    {
                        break;
                    }
                    catch (ObjectDisposedException)
                    {
                        break;
                    }

                    if (context != null)
                    {
                        ThreadPool.QueueUserWorkItem(_ => HandleRequest(context));
                    }
                }
            }

            private void HandleRequest(HttpListenerContext context)
            {
                try
                {
                    string method = (context.Request.HttpMethod ?? string.Empty).ToUpperInvariant();
                    string path = NormalizePath(context.Request.Url.AbsolutePath);

                    if (method == "GET" && path == "/")
                    {
                        WriteJson(context, 200, "\"Hello world\"");
                        return;
                    }

                    if (method == "GET" && path == "/api/v2/pokemon/ditto")
                    {
                        WriteJson(context, 200,
                            "{\"id\":132,\"name\":\"ditto\",\"order\":214,\"species\":{\"name\":\"ditto\",\"url\":\"https://pokeapi.co/api/v2/pokemon-species/132/\"}}");
                        return;
                    }

                    if (method == "GET" && path == "/api/v2/pokemon/meloinvento")
                    {
                        WriteJson(context, 404, "{\"detail\":\"Not Found\"}");
                        return;
                    }

                    if (method == "GET" && path == "/rest/api/latest/issue/jra-meloinvento")
                    {
                        WriteJson(context, 404,
                            "{\"errorMessages\":[\"Issue does not exist or you do not have permission to see it.\"],\"errors\":{}}");
                        return;
                    }

                    if (method == "GET" && path == "/bin/df9f78ca-6298-4a32-93ee-c9130807d116")
                    {
                        WriteJson(context, 200, "\"Hello world\"");
                        return;
                    }

                    if (method == "POST" && path == "/public/v1/users")
                    {
                        HandleCreateUser(context);
                        return;
                    }

                    if (method == "DELETE" && path.StartsWith("/public/v1/users/", StringComparison.Ordinal))
                    {
                        HandleDeleteUser(context, path);
                        return;
                    }

                    WriteJson(context, 404, "{\"detail\":\"Not Found\"}");
                }
                catch
                {
                    TryWriteInternalServerError(context);
                }
            }

            private void HandleCreateUser(HttpListenerContext context)
            {
                string requestBody = ReadRequestBody(context.Request);
                string name = TryReadStringProperty(requestBody, "name");
                string gender = TryReadStringProperty(requestBody, "gender");
                string email = TryReadStringProperty(requestBody, "email");
                string status = TryReadStringProperty(requestBody, "status");

                if (IsNullOrWhiteSpace(name))
                {
                    WriteJson(context, 422,
                        "{\"meta\":null,\"data\":[{\"field\":\"name\",\"message\":\"can't be blank\"}]}");
                    return;
                }

                GoUser user;
                lock (_usersLock)
                {
                    _lastUserId++;
                    user = new GoUser
                    {
                        Id = _lastUserId,
                        Name = name,
                        Gender = gender,
                        Email = email,
                        Status = status
                    };

                    _users[_lastUserId] = user;
                }

                WriteJson(context, 201,
                    "{\"meta\":null,\"data\":{\"id\":" + user.Id.ToString(CultureInfo.InvariantCulture) +
                    ",\"name\":\"" + JsonEscape(user.Name) +
                    "\",\"email\":\"" + JsonEscape(user.Email) +
                    "\",\"gender\":\"" + JsonEscape(user.Gender) +
                    "\",\"status\":\"" + JsonEscape(user.Status) + "\"}}");
            }

            private void HandleDeleteUser(HttpListenerContext context, string path)
            {
                string idSegment = path.Substring("/public/v1/users/".Length);
                int id;
                if (!int.TryParse(idSegment, NumberStyles.Integer, CultureInfo.InvariantCulture, out id))
                {
                    WriteJson(context, 404, "{\"meta\":null,\"data\":{\"message\":\"Resource not found\"}}");
                    return;
                }

                bool removed;
                lock (_usersLock)
                {
                    removed = _users.Remove(id);
                }

                if (!removed)
                {
                    WriteJson(context, 404, "{\"meta\":null,\"data\":{\"message\":\"Resource not found\"}}");
                    return;
                }

                WriteJson(context, 200, "{\"meta\":null,\"data\":{\"message\":\"Resource deleted\"}}");
            }

            private static string NormalizePrefix(Uri serviceUri)
            {
                string prefix = serviceUri.GetLeftPart(UriPartial.Authority);
                if (!prefix.EndsWith("/", StringComparison.Ordinal))
                {
                    prefix += "/";
                }

                return prefix;
            }

            private static string NormalizePath(string rawPath)
            {
                if (string.IsNullOrEmpty(rawPath))
                {
                    return "/";
                }

                string path = rawPath.Trim();
                if (!path.StartsWith("/", StringComparison.Ordinal))
                {
                    path = "/" + path;
                }

                if (path.Length > 1 && path.EndsWith("/", StringComparison.Ordinal))
                {
                    path = path.Substring(0, path.Length - 1);
                }

                return path.ToLowerInvariant();
            }

            private static string ReadRequestBody(HttpListenerRequest request)
            {
                string contentType = request.ContentType ?? string.Empty;
                Encoding encoding = request.ContentEncoding ?? Encoding.UTF8;

                if (contentType.IndexOf("application/json", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    encoding = Encoding.UTF8;
                }

                using (var reader = new StreamReader(request.InputStream, encoding))
                {
                    return reader.ReadToEnd();
                }
            }

            private static string TryReadStringProperty(string json, string propertyName)
            {
                if (string.IsNullOrEmpty(json) || string.IsNullOrEmpty(propertyName))
                {
                    return null;
                }

                string pattern = "\"" + Regex.Escape(propertyName) + "\"\\s*:\\s*(null|\"((?:\\\\.|[^\\\"])*)\")";
                Match match = Regex.Match(json, pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

                if (!match.Success)
                {
                    return null;
                }

                if (string.Equals(match.Groups[1].Value, "null", StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }

                return JsonUnescape(match.Groups[2].Value);
            }

            private static string JsonUnescape(string value)
            {
                if (string.IsNullOrEmpty(value))
                {
                    return value;
                }

                var sb = new StringBuilder(value.Length);

                for (int i = 0; i < value.Length; i++)
                {
                    char c = value[i];
                    if (c != '\\' || i == value.Length - 1)
                    {
                        sb.Append(c);
                        continue;
                    }

                    i++;
                    char esc = value[i];
                    switch (esc)
                    {
                        case '"': sb.Append('"'); break;
                        case '\\': sb.Append('\\'); break;
                        case '/': sb.Append('/'); break;
                        case 'b': sb.Append('\b'); break;
                        case 'f': sb.Append('\f'); break;
                        case 'n': sb.Append('\n'); break;
                        case 'r': sb.Append('\r'); break;
                        case 't': sb.Append('\t'); break;
                        case 'u':
                            if (i + 4 < value.Length)
                            {
                                string hex = value.Substring(i + 1, 4);
                                int code;
                                if (int.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out code))
                                {
                                    sb.Append((char)code);
                                    i += 4;
                                    break;
                                }
                            }
                            sb.Append('u');
                            break;
                        default:
                            sb.Append(esc);
                            break;
                    }
                }

                return sb.ToString();
            }

            private static string JsonEscape(string value)
            {
                if (value == null)
                {
                    return string.Empty;
                }

                var sb = new StringBuilder(value.Length + 8);
                for (int i = 0; i < value.Length; i++)
                {
                    char c = value[i];
                    switch (c)
                    {
                        case '\\': sb.Append("\\\\"); break;
                        case '"': sb.Append("\\\""); break;
                        case '\b': sb.Append("\\b"); break;
                        case '\f': sb.Append("\\f"); break;
                        case '\n': sb.Append("\\n"); break;
                        case '\r': sb.Append("\\r"); break;
                        case '\t': sb.Append("\\t"); break;
                        default:
                            if (c < 32)
                            {
                                sb.Append("\\u");
                                sb.Append(((int)c).ToString("x4", CultureInfo.InvariantCulture));
                            }
                            else
                            {
                                sb.Append(c);
                            }
                            break;
                    }
                }

                return sb.ToString();
            }

            private static void WriteJson(HttpListenerContext context, int statusCode, string json)
            {
                byte[] payload = Encoding.UTF8.GetBytes(json ?? "null");
                context.Response.StatusCode = statusCode;
                context.Response.ContentType = "application/json; charset=utf-8";
                context.Response.ContentLength64 = payload.LongLength;

                using (Stream output = context.Response.OutputStream)
                {
                    output.Write(payload, 0, payload.Length);
                }
            }

            private static void TryWriteInternalServerError(HttpListenerContext context)
            {
                try
                {
                    WriteJson(context, 500, "{\"detail\":\"Internal Server Error\"}");
                }
                catch
                {
                }
            }

            private sealed class GoUser
            {
                public int Id { get; set; }
                public string Name { get; set; }
                public string Gender { get; set; }
                public string Email { get; set; }
                public string Status { get; set; }
            }
        }
    }
}
