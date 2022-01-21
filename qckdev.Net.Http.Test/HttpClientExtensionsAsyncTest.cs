using Microsoft.VisualStudio.TestTools.UnitTesting;
using qckdev.Net.Http.Test.TestObjects;
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
                    new { id = 132, name = "ditto", order = 203, spices = new { name = "ditto", url = "https://pokeapi.co/api/v2/pokemon-species/132/" } },
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
                var rdo = await client.FetchAsync<Pokemon>(HttpMethod.Get, "pokemon/ditto");

                Assert.AreEqual(
                    new { Id = 132, Name = "ditto", Order = 203, Spices = new { Name = "ditto", Url = "https://pokeapi.co/api/v2/pokemon-species/132/" } },
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
                    await client.FetchAsync<Pokemon>(HttpMethod.Get, "pokemon/meloinvento");
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
                    await client.FetchAsync<JiraIssue, JiraError>(HttpMethod.Get, "latest/issue/JRA-meloinvento");
                }
                catch (FetchFailedException<JiraError> ex)
                {
                    Assert.AreEqual(
                        new { StatusCode = (HttpStatusCode?)HttpStatusCode.NotFound, ErrorMessages = "Issue Does Not Exist", Errors = new { } },
                        new { StatusCode = ex.StatusCode, ErrorMessages = string.Join(",", ex.Error.ErrorMessages), Errors = new { } }
                    );
                }
                catch (Exception ex)
                {
                    Assert.ThrowsException<FetchFailedException<JiraError>>(() => throw ex);
                }
            }
        }

        [TestMethod]
        public async Task FetchAsync_Post_Content()
        {
            using (var client = new HttpClient() { BaseAddress = new Uri(Settings.GorestUrl) })
            {
                DateTime momento = DateTime.Now;
                GoResponse<GoUser> rdo;

                var request = new GoUser
                {
                    Name = $"Test {momento}",
                    Gender = "male",
                    Email = $"test.{momento:yyyyMMddhhmmssfff}@somedomain.com",
                    Status = "active"
                };

                client.DefaultRequestHeaders.Authorization
                    = new System.Net.Http.Headers.AuthenticationHeaderValue(
                        "Bearer", Settings.GorestToken);

                rdo = await client.FetchAsync<GoResponse<GoUser>, GoResponse<GoResponseMessage>>(
                HttpMethod.Post, "public/v1/users", request);

                Assert.AreEqual(
                    new { request.Name, request.Gender, request.Email, request.Status },
                    new { rdo.Data.Name, rdo.Data.Gender, rdo.Data.Email, rdo.Data.Status }
                );
            }
        }

        [TestMethod]
        public async Task FetchAsync_CustomDeserializer()
        {
            using (var client = new HttpClient() { BaseAddress = new Uri(Settings.PokemonUrl) })
            {
                var rdo = await client.FetchAsync<Pokemon>(HttpMethod.Get, "pokemon/ditto", options: new FetchAsyncOptions<Pokemon>
                {
                    OnDeserializeAsync = (content) => Task.Factory.StartNew(() => Newtonsoft.Json.JsonConvert.DeserializeObject<Pokemon>(content)),
                    OnDeserializeErrorAsync = (content) => Task.Factory.StartNew(() => Newtonsoft.Json.JsonConvert.DeserializeObject<ExpandoObject>(content))
                });

                Assert.AreEqual(
                    new { Id = 132, Name = "ditto", Order = 203, Spices = new { Name = "ditto", Url = "https://pokeapi.co/api/v2/pokemon-species/132/" } },
                    new { rdo.Id, rdo.Name, rdo.Order, Spices = new { rdo.Species.Name, rdo.Species.Url } }
                );
            }
        }

    }
}
