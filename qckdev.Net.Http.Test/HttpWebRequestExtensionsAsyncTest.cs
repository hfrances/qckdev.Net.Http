using qckdev.Net.Http.Test.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Configuration = qckdev.Net.Http.Test.Common.Configuration;
using TestObjects = qckdev.Net.Http.Test.Common.TestObjects;
using System.Net;
using System.Threading.Tasks;

namespace qckdev.Net.Http.Test
{

    [TestClass]
    public class HttpWebRequestExtensionsAsyncTest
    {

        static Configuration.Settings Settings { get; }

        static HttpWebRequestExtensionsAsyncTest()
        {
            Helpers.SetDefaultSecurityProtocol();
            Settings = Helpers.GetSettings();
        }

        [TestMethod]
        public async Task FetchAsync_Get()
        {
            var request = (HttpWebRequest)WebRequest.Create(new Uri(new Uri(Settings.PokemonUrl), "pokemon/ditto"));

            var rdo = await request.FetchAsync<TestObjects.Pokemon, object>();

            Assert.AreEqual(
                    new { Id = 132, Name = "ditto", Order = 214, Spices = new { Name = "ditto", Url = "https://pokeapi.co/api/v2/pokemon-species/132/" } },
                    new { rdo.Id, rdo.Name, rdo.Order, Spices = new { rdo.Species.Name, rdo.Species.Url } }
                );
        }

        [TestMethod]
        public async Task FetchAsync_Get_String()
        {
            Assert.Inconclusive();
        }

        [TestMethod]
        public async Task FetchAsync_Get_NotFound()
        {
            var request = (HttpWebRequest)WebRequest.Create(new Uri(new Uri(Settings.PokemonUrl), "pokemon/meloinvento"));

            try
            {
                await request.FetchAsync<TestObjects.Pokemon, object>();
            }
            catch (FetchFailedException ex)
            {
                Assert.AreEqual(HttpStatusCode.NotFound, ex.StatusCode);
            }
        }

        [TestMethod]
        public async Task FetchAsync_Get_NotFound_Content_Dynamic()
        {
            var request = (HttpWebRequest)WebRequest.Create(new Uri(new Uri(Settings.JiraUrl), "latest/issue/JRA-meloinvento"));

            try
            {
                await request.FetchAsync<TestObjects.JiraIssue, object>();
            }
            catch (FetchFailedException ex)
            {
                Assert.AreEqual(HttpStatusCode.NotFound, ex.StatusCode);
            }
        }

        [TestMethod]
        public async Task FetchAsync_Get_NotFound_Content()
        {
            var request = (HttpWebRequest)WebRequest.Create(new Uri(new Uri(Settings.JiraUrl), "latest/issue/JRA-meloinvento"));

            try
            {
                await request.FetchAsync<TestObjects.JiraIssue, TestObjects.JiraError>();
            }
            catch (FetchFailedException<TestObjects.JiraError> ex)
            {
                Assert.AreEqual(
                    new { StatusCode = (HttpStatusCode?)HttpStatusCode.NotFound, ErrorMessages = "Issue Does Not Exist", Errors = new { } },
                    new { StatusCode = ex.StatusCode, ErrorMessages = string.Join(",", ex.Error.ErrorMessages.ToArray()), Errors = new { } }
                );
            }
        }

        [TestMethod]
        public async Task FetchAsync_Post_Content()
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
            var request = (HttpWebRequest)WebRequest.Create(new Uri(new Uri(Settings.GorestUrl), "public/v1/users"));

            request.Headers.Add(HttpRequestHeader.Authorization, $"Bearer {Settings.GorestToken}");
            request.Method = "POST";
            request.SetContent(content);

            rdo = await request.FetchAsync<TestObjects.GoResponse<TestObjects.GoUser>, TestObjects.GoResponse<TestObjects.GoResponseMessage>>();
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

        [TestMethod]
        public async Task FetchAsync_Delete()
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

            // Create
            var request = (HttpWebRequest)WebRequest.Create(new Uri(new Uri(Settings.GorestUrl), "public/v1/users"));

            request.Headers.Add(HttpRequestHeader.Authorization, $"Bearer {Settings.GorestToken}");
            request.Method = "POST";
            request.SetContent(content);
            rdo = await request.FetchAsync<TestObjects.GoResponse<TestObjects.GoUser>, TestObjects.GoResponse<TestObjects.GoResponseMessage>>();

            // Delete
            var requestDelete = (HttpWebRequest)WebRequest.Create(new Uri(new Uri(Settings.GorestUrl), $"public/v1/users/{rdo.Data.Id}"));

            requestDelete.Headers.Add(HttpRequestHeader.Authorization, $"Bearer {Settings.GorestToken}");
            requestDelete.Method = "DELETE";
            requestDelete.Fetch<object, TestObjects.GoResponse<TestObjects.GoResponseMessage>>();
        }

        [TestMethod]
        public async Task FetchAsync_Delete_NotFound()
        {
            try
            {
                var requestDelete = (HttpWebRequest)WebRequest.Create(new Uri(new Uri(Settings.GorestUrl), $"public/v1/users/0"));

                requestDelete.Headers.Add(HttpRequestHeader.Authorization, $"Bearer {Settings.GorestToken}");
                requestDelete.Method = "DELETE";
                await requestDelete.FetchAsync<object, TestObjects.GoResponse<TestObjects.GoResponseMessage>>();
            }
            catch (FetchFailedException<TestObjects.GoResponse<TestObjects.GoResponseMessage>> ex)
            {
                Assert.AreEqual(
                    new { StatusCode = (HttpStatusCode?)HttpStatusCode.NotFound, ErrorMessage = "Resource not found" },
                    new { StatusCode = ex.StatusCode, ErrorMessage = ex.Error.Data.Message }
                );
            }
        }

    }

}