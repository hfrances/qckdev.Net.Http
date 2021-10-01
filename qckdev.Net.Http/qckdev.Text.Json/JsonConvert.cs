#if NEWTONSOFT
#else

using System.Dynamic;
using System.Text.Json;

namespace qckdev.Text.Json
{
    public static partial class JsonConvert
    {

        static readonly JsonSerializerOptions joptions = new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public static string SerializeObject<TValue>(TValue value)
        {
            return JsonSerializer.Serialize(value, joptions);
        }

        public static object DeserializeObject(string value)
        {
            return JsonSerializer.Deserialize<ExpandoObject>(value);
        }

        public static TValue DeserializeObject<TValue>(string value)
        {
            return JsonSerializer.Deserialize<TValue>(value, joptions);
        }

    }
}

#endif