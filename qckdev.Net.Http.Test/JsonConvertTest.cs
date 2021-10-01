using Microsoft.VisualStudio.TestTools.UnitTesting;
using qckdev.Net.Http.Test.TestObjects;
using qckdev.Text.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;

namespace qckdev.Net.Http.Test
{

    [TestClass]
    public class JsonConvertTest
    {

        [TestMethod]
        public void SerializeAndDeserialize_Temperature()
        {
            var expected = new Temperature()
            {
                Date = DateTime.Parse("2019-08-01T00:00:00-07:00"),
                TemperatureCelsius = 25,
                Summary = "Hot"
            };
            var json = JsonConvert.SerializeObject(expected);
            var value = JsonConvert.DeserializeObject<Temperature>(json);

            Assert.AreEqual(
                new { expected.Date, expected.TemperatureCelsius, expected.Summary },
                new { value.Date, value.TemperatureCelsius, value.Summary }
            );
        }

        [TestMethod]
        public void Deserialize_Pokemon()
        {
            var rdo = JsonConvert.DeserializeObject<Pokemon>(@"{""id"":132, ""name"":""ditto"", ""order"":203, ""weight"":40, ""species"": {""name"":""ditto"", ""url"":""https://pokeapi.co/api/v2/pokemon-species/132/""} }");

            Assert.AreEqual(
                new { Id = 132, Name = "ditto", Order = 203, Spices = new { Name = "ditto", Url = "https://pokeapi.co/api/v2/pokemon-species/132/" } },
                new { rdo.Id, rdo.Name, rdo.Order, Spices = new { rdo.Species.Name, rdo.Species.Url } }
            );
        }

        [TestMethod]
        public void Deserialize_Dynamic()
        {
#if NEWTONSOFT
            dynamic value = JsonConvert.DeserializeObject(@"{""id"":132, ""name"":""ditto"", ""order"":203, ""weight"":40 }");

            Assert.AreEqual(
                new { Id = 132, Name = "ditto", Order = 203 },
                new { Id = (int)value.id, Name = (string)value.name, Order = (int)value.order }
            );
#else
            Assert.Inconclusive("Pending to implement a dynamic comparer for System.Text.Json."); // TODO: Implement dynamic comprer for System.Text.Json.
#endif
        }

    }
}
