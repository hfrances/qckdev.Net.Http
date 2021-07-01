#if NEWTONSOFT

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;

namespace qckdev.Text.Json
{
    public static partial class JsonConvert
    {

        static readonly JsonSerializerSettings jsettings = new JsonSerializerSettings()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        public static string SerializeObject<TValue>(TValue value)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(value, jsettings);
        }

        public static object DeserializeObject(string value)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject(value, jsettings);
        }

        public static TValue DeserializeObject<TValue>(string value)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<TValue>(value, jsettings);
        }

    }
}

#else
#endif