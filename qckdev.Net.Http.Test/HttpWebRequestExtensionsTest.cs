﻿using qckdev.Net.Http.Test.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Configuration = qckdev.Net.Http.Test.Common.Configuration;
using TestObjects = qckdev.Net.Http.Test.Common.TestObjects;
using System.Net;
using System.Dynamic;

namespace qckdev.Net.Http.Test
{

    [TestClass]
    public class HttpWebRequestExtensionsTest
    {

        static Configuration.Settings Settings { get; }

        static HttpWebRequestExtensionsTest()
        {
            Helpers.SetDefaultSecurityProtocol();
            Settings = Helpers.GetSettings();
        }

        [TestMethod]
        public void Fetch_Get()
        {
            var request = (HttpWebRequest)WebRequest.Create(new Uri(new Uri(Settings.PokemonUrl), "pokemon/ditto"));

            var rdo = request.Fetch<TestObjects.Pokemon, object>();

            Assert.AreEqual(
                    new { Id = 132, Name = "ditto", Order = 214, Spices = new { Name = "ditto", Url = "https://pokeapi.co/api/v2/pokemon-species/132/" } },
                    new { rdo.Id, rdo.Name, rdo.Order, Spices = new { rdo.Species.Name, rdo.Species.Url } }
                );
        }

        [TestMethod]
        public void Fetch_Get_String()
        {
            Assert.Inconclusive();
        }

        [TestMethod]
        public void Fetch_Get_NotFound()
        {
            var request = (HttpWebRequest)WebRequest.Create(new Uri(new Uri(Settings.PokemonUrl), "pokemon/meloinvento"));

            try
            {
                request.Fetch<TestObjects.Pokemon, object>();
            }
            catch (FetchFailedException ex)
            {
                Assert.AreEqual(HttpStatusCode.NotFound, ex.StatusCode);
            }
        }

        [TestMethod]
        public void Fetch_Get_NotFound_Content_Dynamic()
        {
            var request = (HttpWebRequest)WebRequest.Create(new Uri(new Uri(Settings.JiraUrl), "latest/issue/JRA-meloinvento"));

            try
            {
                request.Fetch<TestObjects.JiraIssue, object>();
            }
            catch (FetchFailedException ex)
            {
                Assert.AreEqual(HttpStatusCode.NotFound, ex.StatusCode);
            }
        }

        [TestMethod]
        public void Fetch_Get_NotFound_Content()
        {
            var request = (HttpWebRequest)WebRequest.Create(new Uri(new Uri(Settings.JiraUrl), "latest/issue/JRA-meloinvento"));

            try
            {
                request.Fetch<TestObjects.JiraIssue, TestObjects.JiraError>();
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
        public void Fetch_Get_NotFound_Uri()
        {
            var request = (HttpWebRequest)WebRequest.Create(new Uri(new Uri("http://localhost:5123"), "pokemon/meloinvento"));

            try
            {
                request.Fetch<TestObjects.Pokemon, TestObjects.JiraError>();
            }
            catch (Exception ex)
            {
                Assert.ThrowsException<FetchFailedException<TestObjects.JiraError >> (() => throw ex);
            }
        }

        [TestMethod]
        public void Fetch_Post_Content()
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

            rdo = request.Fetch<TestObjects.GoResponse<TestObjects.GoUser>, TestObjects.GoResponse<TestObjects.GoResponseMessage>>();
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
        public void Fetch_Delete()
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
            rdo = request.Fetch<TestObjects.GoResponse<TestObjects.GoUser>, TestObjects.GoResponse<TestObjects.GoResponseMessage>>();

            // Delete
            var requestDelete = (HttpWebRequest)WebRequest.Create(new Uri(new Uri(Settings.GorestUrl), $"public/v1/users/{rdo.Data.Id}"));

            requestDelete.Headers.Add(HttpRequestHeader.Authorization, $"Bearer {Settings.GorestToken}");
            requestDelete.Method = "DELETE";
            requestDelete.Fetch<object, TestObjects.GoResponse<TestObjects.GoResponseMessage>>();
        }

        [TestMethod]
        public void Fetch_Delete_NotFound()
        {
            try
            {
                var requestDelete = (HttpWebRequest)WebRequest.Create(new Uri(new Uri(Settings.GorestUrl), $"public/v1/users/0"));

                requestDelete.Headers.Add(HttpRequestHeader.Authorization, $"Bearer {Settings.GorestToken}");
                requestDelete.Method = "DELETE";
                requestDelete.Fetch<object, TestObjects.GoResponse<TestObjects.GoResponseMessage>>();
            }
            catch (FetchFailedException<TestObjects.GoResponse<TestObjects.GoResponseMessage>> ex)
            {
                Assert.AreEqual(
                    new { StatusCode = (HttpStatusCode?)HttpStatusCode.NotFound, ErrorMessage = "Resource not found" },
                    new { StatusCode = ex.StatusCode, ErrorMessage = ex.Error.Data.Message }
                );
            }
        }

        [TestMethod]
        public void Fetch_CustomDeserializer()
        {
            var request = (HttpWebRequest)WebRequest.Create(new Uri(new Uri(Settings.PokemonUrl), $"pokemon/ditto"));

            var rdo = request.Fetch<TestObjects.Pokemon>(options: new FetchOptions<TestObjects.Pokemon>
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