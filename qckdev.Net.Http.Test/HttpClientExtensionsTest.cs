using Microsoft.VisualStudio.TestTools.UnitTesting;
using qckdev.Net.Http.Test.TestObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace qckdev.Net.Http.Test
{
    [TestClass]
    public class HttpClientExtensionsTest
    {
        const string URL = "https://pokeapi.co/api/v2/";
        const string URL_JIRA = "https://jira.atlassian.com/rest/api/";

        [TestMethod]
        public async Task Fetch_Get()
        {
            using (var client = new HttpClient() { BaseAddress = new Uri(URL) })
            {
                var rdo = await client.Fetch<Pokemon>(HttpMethod.Get, "pokemon/ditto");

                Assert.AreEqual(
                    new { Id = 132, Name = "ditto", Order = 203, Spices = new { Name = "ditto", Url = "https://pokeapi.co/api/v2/pokemon-species/132/" } },
                    new { rdo.Id, rdo.Name, rdo.Order, Spices = new { rdo.Species.Name, rdo.Species.Url } }
                );
            }
        }

        [TestMethod]
        public async Task Fetch_Get_Dynamic()
        {
            /*
            using (var client = new HttpClient() { BaseAddress = new Uri(URL) })
            {
                var rdo = await client.Fetch(HttpMethod.Get, "pokemon/ditto");

                Assert.AreEqual(
                    new { Id = 132, Name = "ditto", Order = 203 },
                    new { rdo.Id, rdo.Name, rdo.Order }
                );
            }
            */
            Assert.Inconclusive();
        }


        [TestMethod]
        public async Task Fetch_Get_NotFound()
        {
            using (var client = new HttpClient() { BaseAddress = new Uri(URL) })
            {

                try
                {
                    await client.Fetch<Pokemon>(HttpMethod.Get, "pokemon/meloinvento");
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
        public async Task Fetch_Get_NotFound_Content_Dynamic()
        {
            using (var client = new HttpClient() { BaseAddress = new Uri(URL_JIRA) })
            {

                try
                {
                    await client.Fetch<JiraIssue>(HttpMethod.Get, "latest/issue/JRA-meloinvento");
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
        public async Task Fetch_Get_NotFound_Content()
        {
            using (var client = new HttpClient() { BaseAddress = new Uri(URL_JIRA) })
            {

                try 
                {
                    await client.Fetch<JiraIssue, JiraError>(HttpMethod.Get, "latest/issue/JRA-meloinvento");
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

    }
}
