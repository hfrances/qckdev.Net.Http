using Microsoft.VisualStudio.TestTools.UnitTesting;
using Configuration = qckdev.Net.Http.Test.Common.Configuration;
using TestObjects = qckdev.Net.Http.Test.Common.TestObjects;
using qckdev.Net.Http.Test.Common;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace qckdev.Net.Http.Test
{

    [TestClass]
    public class HttpClientExtensionsAsyncTest
    {
        static Configuration.Settings Settings { get; }

        static HttpClientExtensionsAsyncTest()
        {
            Helpers.SetDefaultSecurityProtocol();
            Settings = Helpers.GetSettings();
        }

        [TestMethod]
        public async Task FetchAsync_Get_Dynamic()
        {
#if NO_DYNAMIC
            Assert.Inconclusive("Not dynamic implementation available.");
#else
            using (var client = new HttpClient() { BaseAddress = new Uri(Settings.PokemonUrl) })
            {
                var rdo = await client.FetchAsync(HttpMethod.Get, "pokemon/ditto");

                Assert.AreEqual(
                    new { id = 132, name = "ditto", order = 214, spices = new { name = "ditto", url = "https://pokeapi.co/api/v2/pokemon-species/132/" } },
                    new
                    {
                        id = (int)rdo.id,
                        name = (string)rdo.name,
                        order = (int)rdo.order,
                        spices = new
                        {
                            name = (string)rdo.species.name,
                            url = (string)rdo.species.url
                        }
                    }
                );
            }
#endif
        }

        [TestMethod]
        public async Task FetchAsync_Get()
        {
            using (var client = new HttpClient() { BaseAddress = new Uri(Settings.PokemonUrl) })
            {
                var rdo = await client.FetchAsync<TestObjects.Pokemon>(HttpMethod.Get, "pokemon/ditto");

                Assert.AreEqual(
                    new { Id = 132, Name = "ditto", Order = 214, Spices = new { Name = "ditto", Url = "https://pokeapi.co/api/v2/pokemon-species/132/" } },
                    new { rdo.Id, rdo.Name, rdo.Order, Spices = new { rdo.Species.Name, rdo.Species.Url } }
                );
            }
        }

        [TestMethod]
        public async Task FetchAsync_Get_String()
        {
            Assert.Inconclusive();
        }

        [TestMethod]
        public async Task FetchAsync_Get_NotFound()
        {
            using (var client = new HttpClient() { BaseAddress = new Uri(Settings.PokemonUrl) })
            {

                try
                {
                    await client.FetchAsync<TestObjects.Pokemon>(HttpMethod.Get, "pokemon/meloinvento");
                }
                catch (FetchFailedException ex)
                {
                    Assert.AreEqual(HttpStatusCode.NotFound, ex.StatusCode);
                }
                catch (Exception ex)
                {
                    Assert.ThrowsException<FetchFailedException>(() => throw ex);
                }
            }
        }

        [TestMethod]
        public async Task FetchAsync_Get_NotFound_Content_Dynamic()
        {
#if NO_DYNAMIC
            Assert.Inconclusive("Not dynamic implementation available.");
#else
            using (var client = new HttpClient() { BaseAddress = new Uri(Settings.JiraUrl) })
            {

                try
                {
                    await client.FetchAsync(HttpMethod.Get, "latest/issue/JRA-meloinvento");
                }
                catch (FetchFailedException ex)
                {
                    Assert.AreEqual(HttpStatusCode.NotFound, ex.StatusCode);
                }
                catch (Exception ex)
                {
                    Assert.ThrowsException<FetchFailedException>(() => throw ex);
                }
            }
#endif
        }

        [TestMethod]
        public async Task FetchAsync_Get_NotFound_Content()
        {
            using (var client = new HttpClient() { BaseAddress = new Uri(Settings.JiraUrl) })
            {

                try
                {
                    await client.FetchAsync<TestObjects.JiraIssue, TestObjects.JiraError>(HttpMethod.Get, "latest/issue/JRA-meloinvento");
                }
                catch (FetchFailedException<TestObjects.JiraError> ex)
                {
                    Assert.AreEqual(
                        new { StatusCode = (HttpStatusCode?)HttpStatusCode.NotFound, ErrorMessages = "Issue Does Not Exist", Errors = new { } },
                        new { StatusCode = ex.StatusCode, ErrorMessages = string.Join(",", ex.Error.ErrorMessages), Errors = new { } }
                    );
                }
                catch (Exception ex)
                {
                    Assert.ThrowsException<FetchFailedException<TestObjects.JiraError>>(() => throw ex);
                }
            }
        }

        [TestMethod]
        public async Task FetchAsync_Get_NotFound_Uri()
        {
            using (var client = new HttpClient() { BaseAddress = new Uri("http://localhost:5123/api/") })
            {

                try
                {
                    await client.FetchAsync<TestObjects.JiraIssue, TestObjects.JiraError>(HttpMethod.Get, "latest/issue/JRA-meloinvento");
                }
                catch (Exception ex)
                {
                    Assert.ThrowsException<FetchFailedException<TestObjects.JiraError>>(() => throw ex);
                }
            }
        }

        [TestMethod]
        public async Task FetchAsync_Post_Content()
        {
            using (var client = new HttpClient() { BaseAddress = new Uri(Settings.GorestUrl) })
            {
                DateTime momento = DateTime.Now;
                TestObjects.GoResponse<TestObjects.GoUser> rdo;

                var request = new TestObjects.GoUser
                {
                    Name = $"Test {momento}",
                    Gender = "male",
                    Email = $"test.{momento:yyyyMMddhhmmssfff}@somedomain.com",
                    Status = "active"
                };

                client.DefaultRequestHeaders.Authorization
                    = new System.Net.Http.Headers.AuthenticationHeaderValue(
                        "Bearer", Settings.GorestToken);

                rdo = await client.FetchAsync<TestObjects.GoResponse<TestObjects.GoUser>, TestObjects.GoResponse<TestObjects.GoResponseMessage>>(
                    HttpMethod.Post, "public/v1/users", request
                );

                Assert.AreEqual(
                    new { request.Name, request.Gender, request.Email, request.Status },
                    new { rdo.Data.Name, rdo.Data.Gender, rdo.Data.Email, rdo.Data.Status }
                );
            }
        }

        [TestMethod]
        public async Task FetchAsync_Delete()
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
                rdo = await client.FetchAsync<TestObjects.GoResponse<TestObjects.GoUser>, TestObjects.GoResponse<TestObjects.GoResponseMessage>>(
                    HttpMethod.Post, "public/v1/users", content
                );

                // Delete
                client.Fetch<object, TestObjects.GoResponse<TestObjects.GoResponseMessage>>(HttpMethod.Delete, $"public/v1/users/{rdo.Data.Id}");
            }
        }

        [TestMethod]
        public async Task FetchAsync_Delete_NotFound()
        {
            using (var client = new HttpClient() { BaseAddress = new Uri(Settings.GorestUrl) })
            {
                try
                {
                    await client.FetchAsync<object, TestObjects.GoResponse<TestObjects.GoResponseMessage>>(HttpMethod.Delete, $"public/v1/users/0");
                }
                catch (FetchFailedException<TestObjects.GoResponse<TestObjects.GoResponseMessage>> ex)
                {
                    var exBase = (FetchFailedException)ex;

                    Assert.AreEqual(
                        new { Method = ex.Method, StatusCode = (HttpStatusCode?)HttpStatusCode.NotFound, ErrorMessage = "Resource not found" },
                        new { Method = "DELETE", StatusCode = ex.StatusCode, ErrorMessage = ex.Error.Data.Message }
                    );
                    Assert.AreNotEqual(null, exBase.Error);
                }
            }
        }

        [TestMethod]
        public async Task FetchAsync_CustomDeserializer()
        {
            using (var client = new HttpClient() { BaseAddress = new Uri(Settings.PokemonUrl) })
            {
                var rdo = await client.FetchAsync<TestObjects.Pokemon>(HttpMethod.Get, "pokemon/ditto", options: new FetchAsyncOptions<TestObjects.Pokemon>
                {
                    OnDeserializeAsync = (content) => Task.Factory.StartNew(() => Newtonsoft.Json.JsonConvert.DeserializeObject<TestObjects.Pokemon>(content)),
                    OnDeserializeErrorAsync = (content) => Task.Factory.StartNew(() => Newtonsoft.Json.JsonConvert.DeserializeObject<ExpandoObject>(content))
                });

                Assert.AreEqual(
                    new { Id = 132, Name = "ditto", Order = 214, Spices = new { Name = "ditto", Url = "https://pokeapi.co/api/v2/pokemon-species/132/" } },
                    new { rdo.Id, rdo.Name, rdo.Order, Spices = new { rdo.Species.Name, rdo.Species.Url } }
                );
            }
        }

    }
}
