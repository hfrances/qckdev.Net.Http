using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.Versioning;
using System.Threading;

namespace qckdev.Net.Http.Test.Common
{
    public static class LocalTestServiceManager
    {
        private static readonly object SyncLock = new object();
        private static Process _serviceProcess;
        private static bool _initialized;
        private static bool _ownedProcess;

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

            int port = GetPortForCurrentFramework();
            settings.PokemonUrl = RewriteLocalUrlPort(settings.PokemonUrl, port);
            settings.JiraUrl = RewriteLocalUrlPort(settings.JiraUrl, port);
            settings.GorestUrl = RewriteLocalUrlPort(settings.GorestUrl, port);
            settings.MockbinUrl = RewriteLocalUrlPort(settings.MockbinUrl, port);

            return settings;
        }

        public static void StartIfNeeded(Configuration.Settings settings, string baseDirectory)
        {
            settings = NormalizeSettingsForCurrentFramework(settings);

            if (!ShouldUseLocalService(settings))
            {
                return;
            }
            if (IsAutoStartDisabled())
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
                    _ownedProcess = false;
                    return;
                }

                string projectPath = FindServiceProjectPath(baseDirectory);
                if (string.IsNullOrEmpty(projectPath) || !File.Exists(projectPath))
                {
                    throw new InvalidOperationException(
                        "Cannot locate qckdev.Net.Http.Test.Service.csproj to start local test service.");
                }

                _serviceProcess = StartServiceProcess(projectPath, serviceUri);
                if (_serviceProcess == null)
                {
                    throw new InvalidOperationException("Cannot start local test service process.");
                }

                if (!WaitForService(serviceUri, 30000))
                {
                    TryStopProcess(_serviceProcess);
                    _serviceProcess = null;

                    EnsureServiceBuilt(projectPath);
                    _serviceProcess = StartServiceProcess(projectPath, serviceUri);
                    if (_serviceProcess == null || !WaitForService(serviceUri, 30000))
                    {
                        TryStopProcess(_serviceProcess);
                        _serviceProcess = null;
                        throw new InvalidOperationException(
                            string.Format("Local test service did not become ready at {0}.", serviceUri.GetLeftPart(UriPartial.Authority)));
                    }
                }

                _ownedProcess = true;
                AppDomain.CurrentDomain.ProcessExit += (_, __) => StopIfOwned();
                AppDomain.CurrentDomain.DomainUnload += (_, __) => StopIfOwned();
            }
        }

        public static void StopIfOwned()
        {
            lock (SyncLock)
            {
                if (!_ownedProcess)
                {
                    return;
                }

                try
                {
                    if (_serviceProcess != null && !_serviceProcess.HasExited)
                    {
                        _serviceProcess.Kill();
                        _serviceProcess.Dispose();
                    }
                }
                catch
                {
                    // Ignore shutdown failures in test teardown.
                }
                finally
                {
                    _serviceProcess = null;
                    _ownedProcess = false;
                    _initialized = false;
                }
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
            if (settings == null)
            {
                return false;
            }

            return HasLocalHostTarget(settings);
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
            if (string.IsNullOrWhiteSpace(value))
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

                if (_serviceProcess != null && _serviceProcess.HasExited)
                {
                    return false;
                }

                Thread.Sleep(250);
            }

            return false;
        }

        private static Process StartServiceProcess(string projectPath, Uri serviceUri)
        {
            var startInfo = new ProcessStartInfo("dotnet", string.Format("run --no-build --no-launch-profile --project \"{0}\"", projectPath))
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = Path.GetDirectoryName(projectPath)
            };
            startInfo.EnvironmentVariables["ASPNETCORE_URLS"] = serviceUri.GetLeftPart(UriPartial.Authority);
            return Process.Start(startInfo);
        }

        private static void EnsureServiceBuilt(string projectPath)
        {
            var startInfo = new ProcessStartInfo("dotnet", string.Format("build \"{0}\" -c Debug", projectPath))
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = Path.GetDirectoryName(projectPath)
            };

            using (var buildProcess = Process.Start(startInfo))
            {
                if (buildProcess == null)
                {
                    throw new InvalidOperationException("Cannot build local test service project.");
                }

                buildProcess.WaitForExit();
                if (buildProcess.ExitCode != 0)
                {
                    throw new InvalidOperationException("Cannot build local test service project.");
                }
            }
        }

        private static void TryStopProcess(Process process)
        {
            if (process == null)
            {
                return;
            }

            try
            {
                if (!process.HasExited)
                {
                    process.Kill();
                }
            }
            catch
            {
                // Ignore process stop failures in test startup.
            }
            finally
            {
                process.Dispose();
            }
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
            if (string.IsNullOrWhiteSpace(value))
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
            if (string.IsNullOrWhiteSpace(value))
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

        private static int GetPortForCurrentFramework()
        {
            string tfm = GetCurrentFrameworkName();

            if (tfm.IndexOf(".NETCoreApp", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                if (tfm.IndexOf("v10.0", StringComparison.OrdinalIgnoreCase) >= 0) return 5210;
                if (tfm.IndexOf("v9.0", StringComparison.OrdinalIgnoreCase) >= 0) return 5209;
                if (tfm.IndexOf("v8.0", StringComparison.OrdinalIgnoreCase) >= 0) return 5208;
                if (tfm.IndexOf("v7.0", StringComparison.OrdinalIgnoreCase) >= 0) return 5207;
                if (tfm.IndexOf("v6.0", StringComparison.OrdinalIgnoreCase) >= 0) return 5206;
                if (tfm.IndexOf("v5.0", StringComparison.OrdinalIgnoreCase) >= 0) return 5205;
                if (tfm.IndexOf("v3.1", StringComparison.OrdinalIgnoreCase) >= 0) return 5231;
            }

            if (tfm.IndexOf(".NETFramework", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                if (tfm.IndexOf("v4.6.1", StringComparison.OrdinalIgnoreCase) >= 0) return 5461;
                if (tfm.IndexOf("v4.5.1", StringComparison.OrdinalIgnoreCase) >= 0) return 5451;
                if (tfm.IndexOf("v4.5", StringComparison.OrdinalIgnoreCase) >= 0) return 5450;
                if (tfm.IndexOf("v4.0", StringComparison.OrdinalIgnoreCase) >= 0) return 5400;
                if (tfm.IndexOf("v3.5", StringComparison.OrdinalIgnoreCase) >= 0) return 5350;
            }

            return 5123;
        }

        private static string GetCurrentFrameworkName()
        {
            var attrs = typeof(LocalTestServiceManager).Assembly.GetCustomAttributes(typeof(TargetFrameworkAttribute), false);
            var attr = attrs != null && attrs.Length > 0 ? attrs[0] as TargetFrameworkAttribute : null;

            if (attr != null && !string.IsNullOrEmpty(attr.FrameworkName))
            {
                return attr.FrameworkName;
            }

            return AppDomain.CurrentDomain.SetupInformation.TargetFrameworkName ?? string.Empty;
        }

        private static string FindServiceProjectPath(string baseDirectory)
        {
            var current = new DirectoryInfo(baseDirectory);
            while (current != null)
            {
                string candidate = Path.Combine(
                    current.FullName,
                    "qckdev.Net.Http.Test.Service",
                    "qckdev.Net.Http.Test.Service.csproj");

                if (File.Exists(candidate))
                {
                    return candidate;
                }

                current = current.Parent;
            }

            return null;
        }
    }
}
