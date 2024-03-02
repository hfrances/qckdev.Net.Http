#if NO_SYNC
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Configuration = qckdev.Net.Http.Test.Common.Configuration;
using TestObjects = qckdev.Net.Http.Test.Common.TestObjects;
using qckdev.Net.Http.Test.Common;
using System;
using System.Net;
using System.Net.Http;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading.Tasks;
using qckdev.Text.Json;

namespace qckdev.Net.Http.Test
{
    [TestClass]
    public class HttpClientExtensionsTest
    {
        static Configuration.Settings Settings { get; }

        static HttpClientExtensionsTest()
        {
            Helpers.SetDefaultSecurityProtocol();
            Settings = Helpers.GetSettings();
        }

        [TestMethod]
        public void Fetch_Get()
        {
            using (var client = new HttpClient() { BaseAddress = new Uri(Settings.PokemonUrl) })
            {
                var rdo = client.Fetch<TestObjects.Pokemon>(HttpMethod.Get, "pokemon/ditto");

                Assert.AreEqual(
                    new { Id = 132, Name = "ditto", Order = 214, Spices = new { Name = "ditto", Url = "https://pokeapi.co/api/v2/pokemon-species/132/" } },
                    new { rdo.Id, rdo.Name, rdo.Order, Spices = new { rdo.Species.Name, rdo.Species.Url } }
                );
            }
        }

        [TestMethod]
        public void Fetch_Get_String()
        {
            using (var client = new HttpClient() { BaseAddress = null })
            {
                var rdo = client.Fetch<string>(HttpMethod.Get, Settings.MockbinUrl);

                Assert.AreEqual("Hello world", rdo);
            }
        }

        [TestMethod]
        public void Fetch_Get_Dynamic()
        {
#if NO_DYNAMIC
            Assert.Inconclusive("Not dynamic implementation available.");
#else
            using (var client = new HttpClient() { BaseAddress = new Uri(Settings.PokemonUrl) })
            {
                var rdo = client.Fetch(HttpMethod.Get, "pokemon/ditto");

                Assert.AreEqual(
                    new { Id = 132, Name = "ditto", Order = 214 },
                    new { Id = (int)rdo.id, Name = (string)rdo.name, Order = (int)rdo.order }
                );
            }
#endif
        }

        [TestMethod]
        public void Fetch_Get_NotFound()
        {
            using (var client = new HttpClient() { BaseAddress = new Uri(Settings.PokemonUrl) })
            {

                try
                {
                    client.Fetch<TestObjects.Pokemon>(HttpMethod.Get, "pokemon/meloinvento");
                }
                catch (FetchFailedException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    Assert.ThrowsException<FetchFailedException<ExpandoObject>>(() => throw ex);
                }
            }
        }

        [TestMethod]
        public void Fetch_Get_NotFound_Content_Dynamic()
        {
            using (var client = new HttpClient() { BaseAddress = new Uri(Settings.JiraUrl) })
            {

                try
                {
                    client.Fetch<TestObjects.JiraIssue>(HttpMethod.Get, "latest/issue/JRA-meloinvento");
                }
                catch (FetchFailedException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    Assert.ThrowsException<FetchFailedException<ExpandoObject>>(() => throw ex);
                }
            }
        }

        [TestMethod]
        public void Fetch_Get_NotFound_Content()
        {
            using (var client = new HttpClient() { BaseAddress = new Uri(Settings.JiraUrl) })
            {

                try
                {
                    client.Fetch<TestObjects.JiraIssue, TestObjects.JiraError>(HttpMethod.Get, "latest/issue/JRA-meloinvento");
                }
                catch (FetchFailedException<TestObjects.JiraError> ex) when (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    Assert.AreEqual(
                        new { StatusCode = (HttpStatusCode?)HttpStatusCode.NotFound, ErrorMessages = "Issue does not exist or you do not have permission to see it.", Errors = new { } },
                        new { StatusCode = ex.StatusCode, ErrorMessages = string.Join(",", ex.Error?.ErrorMessages ?? new string[] { }), Errors = new { } }
                    );
                }
            }
        }

        [TestMethod]
        public void Fetch_Get_SocketConnection()
        {
            using (var client = new HttpClient() { BaseAddress = new Uri("http://localhost:5123/api/") })
            {

                try
                {
                    client.Fetch<TestObjects.JiraIssue, TestObjects.JiraError>(HttpMethod.Get, "latest/issue/JRA-meloinvento");
                }
                catch (FetchFailedException<TestObjects.JiraError> ex)
                {
                    Assert.ThrowsException<FetchFailedException<TestObjects.JiraError>>(() => throw ex);
                }
            }
        }

        [TestMethod]
        public void Fetch_Post_Content()
        {
            using (var client = new HttpClient() { BaseAddress = new Uri(Settings.GorestUrl) })
            {
                DateTime momento = DateTime.Now;
                TestObjects.GoResponse<TestObjects.GoUser> rdo;

                var request = new TestObjects.GoUser
                {
                    Name = $"Héctor {momento}", // Usar acento para verificar que funciona el encoding.
                    Gender = "male",
                    Email = $"test.{momento:yyyyMMddhhmmssfff}@somedomain.com",
                    Status = "active"
                };

                client.DefaultRequestHeaders.Authorization
                    = new System.Net.Http.Headers.AuthenticationHeaderValue(
                        "Bearer", Settings.GorestToken);
                rdo = client.Fetch<TestObjects.GoResponse<TestObjects.GoUser>, TestObjects.GoResponse<TestObjects.GoResponseMessage>>(
                    HttpMethod.Post, "public/v1/users", request);

                Assert.AreEqual(
                    new { request.Name, request.Gender, request.Email, request.Status },
                    new { rdo.Data.Name, rdo.Data.Gender, rdo.Data.Email, rdo.Data.Status }
                );
            }
        }

        [TestMethod]
        public void Fetch_Post_Content_UnprocessableEntity()
        {
            using (var client = new HttpClient() { BaseAddress = new Uri(Settings.GorestUrl) })
            {
                DateTime momento = DateTime.Now;
                TestObjects.GoResponse<TestObjects.GoUser> rdo;

                var request = new TestObjects.GoUser
                {
                    Name = null, // force error.
                    Gender = "male",
                    Email = $"test.{momento:yyyyMMddhhmmssfff}@somedomain.com",
                    Status = "active"
                };

                client.DefaultRequestHeaders.Authorization
                    = new System.Net.Http.Headers.AuthenticationHeaderValue(
                        "Bearer", Settings.GorestToken);

                try
                {
                    rdo = client.Fetch<TestObjects.GoResponse<TestObjects.GoUser>, TestObjects.GoResponse>(
                        HttpMethod.Post, "public/v1/users", request
                    );
                }
                catch (FetchFailedException<TestObjects.GoResponse> ex) when ((int?)ex.StatusCode == 422) // UnprocessableEntity
                {

                    Assert.AreEqual(
                        new
                        {
                            Method = "POST",
                            RequestUri = new Uri(client.BaseAddress, "public/v1/users"),
                            RequestContentType = "application/json; charset=utf-8",
                            RequestContent = JsonConvert.SerializeObject(request),
                            StatusCode = (int?)422,
                            Message = "Unprocessable Entity",
                            Error = JsonConvert.SerializeObject(new
                            {
                                Meta = (string)null,
                                Data = new[] { new { field = "name", message = "can't be blank" } }
                            })
                        },
                        new
                        {
                            Method = ex.Method,
                            RequestUri = ex.RequestUri,
                            RequestContentType = ex.RequestContentType,
                            RequestContent = ex.RequestContent,
                            StatusCode = (int?)ex.StatusCode,
                            Message = ex.Message,
                            Error = JsonConvert.SerializeObject(ex.Error)
                        }
                    );
                }
            }
        }

        [TestMethod]
        public void Fetch_Delete()
        {
            using (var client = new HttpClient() { BaseAddress = new Uri(Settings.GorestUrl) })
            {
                DateTime momento = DateTime.Now;
                TestObjects.GoResponse<TestObjects.GoUser> rdo;
                var content = new TestObjects.GoUser
                {
                    Name = $"Test {momento}",
                    Gender = "male",
                    Email = $"test.{momento:yyyyMMddhhmmssfff}@somedomain.com",
                    Status = "active"
                };

                client.DefaultRequestHeaders.Authorization
                    = new System.Net.Http.Headers.AuthenticationHeaderValue(
                        "Bearer", Settings.GorestToken);

                // Create
                rdo = client.Fetch<TestObjects.GoResponse<TestObjects.GoUser>, TestObjects.GoResponse<TestObjects.GoResponseMessage>>(
                    HttpMethod.Post, "public/v1/users", content
                );

                // Delete
                client.Fetch<object, TestObjects.GoResponse<TestObjects.GoResponseMessage>>(HttpMethod.Delete, $"public/v1/users/{rdo.Data.Id}");
            }
        }

        [TestMethod]
        public void Fetch_Delete_NotFound()
        {
            using (var client = new HttpClient() { BaseAddress = new Uri(Settings.GorestUrl) })
            {
                try
                {
                    client.Fetch<object, TestObjects.GoResponse<TestObjects.GoResponseMessage>>(HttpMethod.Delete, $"public/v1/users/0");
                }
                catch (FetchFailedException<TestObjects.GoResponse<TestObjects.GoResponseMessage>> ex) when (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    Assert.AreEqual(
                        new { Method = "DELETE", StatusCode = (HttpStatusCode?)HttpStatusCode.NotFound, ErrorMessage = "Resource not found" },
                        new { Method = ex.Method, StatusCode = ex.StatusCode, ErrorMessage = ex.Error.Data.Message }
                    );
                }
            }
        }

        [TestMethod]
        public void Fetch_CustomDeserializer()
        {
            using (var client = new HttpClient() { BaseAddress = new Uri(Settings.PokemonUrl) })
            {
                var rdo = client.Fetch<TestObjects.Pokemon>(HttpMethod.Get, "pokemon/ditto", options: new FetchOptions<TestObjects.Pokemon>
                {
                    OnDeserialize = (content) => Newtonsoft.Json.JsonConvert.DeserializeObject<TestObjects.Pokemon>(content),
                    OnDeserializeError = (content) => Newtonsoft.Json.JsonConvert.DeserializeObject<ExpandoObject>(content)
                });

                Assert.AreEqual(
                    new { Id = 132, Name = "ditto", Order = 214, Spices = new { Name = "ditto", Url = "https://pokeapi.co/api/v2/pokemon-species/132/" } },
                    new { rdo.Id, rdo.Name, rdo.Order, Spices = new { rdo.Species.Name, rdo.Species.Url } }
                );
            }
        }

    }
}
#endif