using qckdev.Net.Http.Test.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Configuration = qckdev.Net.Http.Test.Common.Configuration;
using TestObjects = qckdev.Net.Http.Test.Common.TestObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace qckdev.Net.Http.Test.Net35
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
            Assert.Inconclusive();
        }

        [TestMethod]
        public void Fetch_Get_NotFound()
        {
            using (var client = new WebClient() { BaseAddress = Settings.PokemonUrl })
            {

                try
                {
                    client.Fetch<TestObjects.Pokemon, object>("GET", "pokemon/meloinvento");
                }
                catch (FetchFailedException ex)
                {
                    Assert.AreEqual(HttpStatusCode.NotFound, ex.StatusCode);
                }
            }
        }

        [TestMethod]
        public void Fetch_Get_NotFound_Content_Dynamic()
        {
            using (var client = new WebClient() { BaseAddress = Settings.JiraUrl })
            {

                try
                {
                    client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                    client.Headers.Add("charset", "'utf-8'");
                    client.Fetch<TestObjects.Pokemon, object>("GET", "latest/issue/JRA-meloinvento");
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
                catch (FetchFailedException<TestObjects.JiraError> ex)
                {
                    Assert.AreEqual(
                        new { StatusCode = (HttpStatusCode?)HttpStatusCode.NotFound, ErrorMessages = "Issue Does Not Exist", Errors = new { } },
                        new { StatusCode = ex.StatusCode, ErrorMessages = string.Join(",", ex.Error.ErrorMessages.ToArray()), Errors = new { } }
                    );
                }
            }
        }

        [TestMethod]
        public void Fetch_Get_NotFound_Uri()
        {
            using (var client = new WebClient() { BaseAddress = "http://localhost:5123" })
            {
                try
                {
                    client.Fetch<TestObjects.Pokemon, TestObjects.JiraError>("GET", "pokemon/meloinvento");
                }
                catch (FetchFailedException<TestObjects.JiraError>)
                {
                    Assert.IsTrue(true);
                }
            }
        }

        [TestMethod]
        public void Fetch_Post_Content()
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
                catch (FetchFailedException<TestObjects.GoResponse<TestObjects.GoResponseMessage>> ex)
                {
                    Assert.AreEqual(
                        new { StatusCode = (HttpStatusCode?)HttpStatusCode.NotFound, ErrorMessage = "Resource not found" },
                        new { StatusCode = ex.StatusCode, ErrorMessage = ex.Error.Data.Message }
                    );
                }
            }
        }

        [TestMethod]
        public void Fetch_CustomDeserializer()
        {
            using (var client = new WebClient() { BaseAddress = Settings.PokemonUrl })
            {
                var rdo = client.Fetch<TestObjects.Pokemon, object>("GET", $"pokemon/ditto", options: new FetchOptions<TestObjects.Pokemon, object>
                {
                    OnDeserialize = (content) => Newtonsoft.Json.JsonConvert.DeserializeObject<TestObjects.Pokemon>(content),
                    OnDeserializeError = (content) => Newtonsoft.Json.JsonConvert.DeserializeObject<object>(content)
                });

                Assert.AreEqual(
                    new { Id = 132, Name = "ditto", Order = 214, Spices = new { Name = "ditto", Url = "https://pokeapi.co/api/v2/pokemon-species/132/" } },
                    new { rdo.Id, rdo.Name, rdo.Order, Spices = new { rdo.Species.Name, rdo.Species.Url } }
                );
            }
        }

    }
}
