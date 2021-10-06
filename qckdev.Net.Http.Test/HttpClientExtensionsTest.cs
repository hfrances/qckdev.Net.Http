#if NO_SYNC
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
using qckdev.Net.Http.Test.TestObjects;
using System;
using System.Net;
using System.Net.Http;
using System.Collections.Generic;

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
                var rdo = client.Fetch<Pokemon>(HttpMethod.Get, "pokemon/ditto");

                Assert.AreEqual(
                    new { Id = 132, Name = "ditto", Order = 203, Spices = new { Name = "ditto", Url = "https://pokeapi.co/api/v2/pokemon-species/132/" } },
                    new { rdo.Id, rdo.Name, rdo.Order, Spices = new { rdo.Species.Name, rdo.Species.Url } }
                );
            }
        }

        [TestMethod]
        public void Fetch_Get_Dynamic()
        {
#if NEWTONSOFT
            using (var client = new HttpClient() { BaseAddress = new Uri(Settings.PokemonUrl) })
            {
                var rdo = client.Fetch(HttpMethod.Get, "pokemon/ditto");

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
        public void Fetch_Get_NotFound()
        {
            using (var client = new HttpClient() { BaseAddress = new Uri(Settings.PokemonUrl) })
            {

                try
                {
                    client.Fetch<Pokemon>(HttpMethod.Get, "pokemon/meloinvento");
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
        public void Fetch_Get_NotFound_Content_Dynamic()
        {
            using (var client = new HttpClient() { BaseAddress = new Uri(Settings.JiraUrl) })
            {

                try
                {
                    client.Fetch<JiraIssue>(HttpMethod.Get, "latest/issue/JRA-meloinvento");
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
        public void Fetch_Get_NotFound_Content()
        {
            using (var client = new HttpClient() { BaseAddress = new Uri(Settings.JiraUrl) })
            {

                try
                {
                    client.Fetch<JiraIssue, JiraError>(HttpMethod.Get, "latest/issue/JRA-meloinvento");
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
        public void Fetch_Post_Content()
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
                rdo = client.Fetch<GoResponse<GoUser>, GoResponse<IEnumerable<GoError>>>(
                    HttpMethod.Post, "public/v1/users", request);

                Assert.AreEqual(
                    new { request.Name, request.Gender, request.Email, request.Status },
                    new { rdo.Data.Name, rdo.Data.Gender, rdo.Data.Email, rdo.Data.Status }
                );
            }
        }
    }
}
#endif