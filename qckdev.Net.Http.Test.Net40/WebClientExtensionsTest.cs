using qckdev.Net.Http.Test.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Configuration = qckdev.Net.Http.Test.Common.Configuration;
using TestObjects = qckdev.Net.Http.Test.Common.TestObjects;
using System;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Dynamic;

namespace qckdev.Net.Http.Test.Net40
{

    [TestClass]
    public class WebClientExtensionsTest
    {

        static Configuration.Settings Settings { get; }

        static WebClientExtensionsTest()
        {
            Helpers.SetDefaultSecurityProtocol();
            Settings = Helpers.GetSettings();
        }

        [TestMethod]
        public void Fetch_Get()
        {
            using (var client = new WebClient() { BaseAddress = Settings.PokemonUrl })
            {
                var rdo = client.Fetch<TestObjects.Pokemon, object>("GET", "pokemon/ditto");

                Assert.AreEqual(
                        new { Id = 132, Name = "ditto", Order = 214, Spices = new { Name = "ditto", Url = "https://pokeapi.co/api/v2/pokemon-species/132/" } },
                        new { rdo.Id, rdo.Name, rdo.Order, Spices = new { rdo.Species.Name, rdo.Species.Url } }
                    );
            }
        }

        [TestMethod]
        public void Fetch_Get_String()
        {
            using (var client = new WebClient() { BaseAddress = Settings.MockbinUrl })
            {
                var rdo = client.Fetch<string>("GET", "bin/df9f78ca-6298-4a32-93ee-c9130807d116");

                Assert.AreEqual("Hello world", rdo);
            }
        }

        [TestMethod]
        public void Fetch_Get_NotFound()
        {
            using (var client = new WebClient() { BaseAddress = Settings.PokemonUrl })
            {

                try
                {
                    client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                    client.Headers.Add("charset", "utf-8");
                    client.Fetch<TestObjects.Pokemon>("GET", "pokemon/meloinvento");
                }
                catch (FetchFailedException ex)
                {
                    Assert.AreEqual(HttpStatusCode.NotFound, ex.StatusCode);
                }
            }
        }

        [TestMethod]
        public void Fetch_Get_NotFound_Content()
        {
            using (var client = new WebClient() { BaseAddress = Settings.JiraUrl })
            {

                try
                {
                    client.Fetch<TestObjects.JiraIssue, TestObjects.JiraError>("GET", "latest/issue/JRA-meloinvento");
                }
                catch (FetchFailedException<TestObjects.JiraError> ex) when (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    Assert.AreEqual(
                        new { StatusCode = (HttpStatusCode?)HttpStatusCode.NotFound, ErrorMessages = "Issue does not exist or you do not have permission to see it.", Errors = new { } },
                        new { StatusCode = ex.StatusCode, ErrorMessages = string.Join(",", ex.Error?.ErrorMessages.ToArray() ?? new string[] { }), Errors = new { } }
                    );
                }
            }
        }

        [TestMethod]
        public void Fetch_Get_SocketConnection()
        {
            using (var client = new WebClient() { BaseAddress = "http://localhost:5123" })
            {
                try
                {
                    client.Fetch<TestObjects.Pokemon>("GET", "pokemon/meloinvento");
                }
                catch (FetchFailedException)
                {
                    Assert.IsTrue(true);
                }
            }
        }

        [TestMethod]
        public void Fetch_Post_Content()
        {
            using (var client = new WebClient() { BaseAddress = Settings.GorestUrl, Encoding = System.Text.Encoding.UTF8 })
            {
                DateTime momento = DateTime.Now;
                TestObjects.GoResponse<TestObjects.GoUser> rdo;
                var content = new TestObjects.GoUser
                {
                    Name = $"Héctor {momento}", // Usar acento para verificar que funciona el encoding.
                    Gender = "male",
                    Email = $"test.{momento:yyyyMMddhhmmssfff}@somedomain.com",
                    Status = "active"
                };

                client.Headers.Add(HttpRequestHeader.Authorization, $"Bearer {Settings.GorestToken}");
                rdo = client.Fetch<TestObjects.GoResponse<TestObjects.GoUser>, TestObjects.GoResponse<TestObjects.GoResponseMessage>>(
                    "POST", "public/v1/users", content
                );
                Assert.AreEqual(
                    new
                    {
                        content.Name,
                        content.Gender,
                        content.Email,
                        content.Status
                    },
                    new { rdo.Data.Name, rdo.Data.Gender, rdo.Data.Email, rdo.Data.Status }
                );
            }
        }

        [TestMethod]
        public void Fetch_Post_Content_UnprocessableEntity()
        {
            using (var client = new WebClient() { BaseAddress = Settings.GorestUrl })
            {
                DateTime momento = DateTime.Now;
                TestObjects.GoResponse<TestObjects.GoUser> rdo;

                var content = new TestObjects.GoUser
                {
                    Name = null, // force error.
                    Gender = "male",
                    Email = $"test.{momento:yyyyMMddhhmmssfff}@somedomain.com",
                    Status = "active"
                };

                client.Headers.Add(HttpRequestHeader.Authorization, $"Bearer {Settings.GorestToken}");

                try
                {
                    rdo = client.Fetch<TestObjects.GoResponse<TestObjects.GoUser>, TestObjects.GoResponse>(
                        "POST", "public/v1/users", content
                    );
                }
                catch (FetchFailedException<TestObjects.GoResponse> ex) when ((int?)ex.StatusCode == 422) // UnprocessableEntity
                {
                    var serializerSettings = new JsonSerializerSettings { ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() } };

                    Assert.AreEqual(
                        new
                        {
                            Method = "POST",
                            RequestUri = new Uri(new Uri(client.BaseAddress), "public/v1/users"),
                            RequestContentType = "application/json; charset=utf-8",
                            RequestContent = JsonConvert.SerializeObject(content, serializerSettings),
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
            using (var client = new WebClient() { BaseAddress = Settings.GorestUrl })
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

                client.Headers.Add(HttpRequestHeader.Authorization, $"Bearer {Settings.GorestToken}");

                // Create
                rdo = client.Fetch<TestObjects.GoResponse<TestObjects.GoUser>, TestObjects.GoResponse<TestObjects.GoResponseMessage>>(
                    "POST", "public/v1/users", content
                );

                // Delete
                client.Fetch<object, TestObjects.GoResponse<TestObjects.GoResponseMessage>>("DELETE", $"public/v1/users/{rdo.Data.Id}");
            }
        }

        [TestMethod]
        public void Fetch_Delete_NotFound()
        {
            using (var client = new WebClient() { BaseAddress = Settings.GorestUrl })
            {
                try
                {
                    client.Fetch<object, TestObjects.GoResponse<TestObjects.GoResponseMessage>>("DELETE", $"public/v1/users/0");
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
            using (var client = new WebClient() { BaseAddress = Settings.PokemonUrl })
            {
                var rdo = client.Fetch<TestObjects.Pokemon>("GET", $"pokemon/ditto", options: new FetchOptions<TestObjects.Pokemon>
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
