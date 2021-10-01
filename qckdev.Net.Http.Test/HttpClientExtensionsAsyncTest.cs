using Microsoft.VisualStudio.TestTools.UnitTesting;
using qckdev.Net.Http.Test.TestObjects;
using System;
using System.Collections.Generic;
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
        public async Task FetchAsync_Get_Dynamic()
        {
#if NEWTONSOFT
            using (var client = new HttpClient() { BaseAddress = new Uri(Settings.PokemonUrl) })
            {
                var rdo = await client.FetchAsync(HttpMethod.Get, "pokemon/ditto");

                Assert.AreEqual(
                    new { Id = 132, Name = "ditto", Order = 203 },
                    new { Id = (int)rdo.id, Name = (string)rdo.name, Order = (int)rdo.order }
                );
            }
#else
            Assert.Inconclusive("Pending to implement a dynamic comparer for System.Text.Json."); // TODO: Implement dynamic comprer for System.Text.Json.
#endif
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
                catch (FetchFailedException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    Assert.ThrowsException<FetchFailedException>(() => throw ex);
                }
                catch
                {
                    throw;
                }
            }
        }

        [TestMethod]
        public async Task FetchAsync_Get_NotFound_Content_Dynamic()
        {
            using (var client = new HttpClient() { BaseAddress = new Uri(Settings.JiraUrl) })
            {

                try
                {
                    await client.FetchAsync<JiraIssue>(HttpMethod.Get, "latest/issue/JRA-meloinvento");
                }
                catch (FetchFailedException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    Assert.ThrowsException<FetchFailedException>(() => throw ex);
                }
                catch
                {
                    throw;
                }
            }
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
                catch (FetchFailedException<JiraError> ex) when (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    Assert.AreEqual(
                        new { ErrorMessages = "Issue Does Not Exist", Errors = new { } },
                        new { ErrorMessages = string.Join(",", ex.Error.ErrorMessages), Errors = new { } }
                    );
                    Assert.ThrowsException<FetchFailedException<JiraError>>(() =>
                    {
                        throw ex;
                    });
                }
                catch
                {
                    throw;
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
                rdo = await client.FetchAsync<GoResponse<GoUser>, GoResponse<IEnumerable<GoError>>>(
                    HttpMethod.Post, "public/v1/users", request);

                Assert.AreEqual(
                    new { request.Name, request.Gender, request.Email, request.Status },
                    new { rdo.Data.Name, rdo.Data.Gender, rdo.Data.Email, rdo.Data.Status }
                );
            }
        }

    }
}
