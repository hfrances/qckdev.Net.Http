#if NEWTONSOFT
#else

using System.Text.Json;

namespace qckdev.Net.Http
{
    static partial class JsonConvert
    {

        static readonly JsonSerializerOptions joptions = new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public static string SerializeObject<TValue>(TValue value)
        {
            return JsonSerializer.Serialize(value, joptions);
        }

        public static TValue DeserializeObject<TValue>(string value)
        {
            return JsonSerializer.Deserialize<TValue>(value, joptions);
        }

    }
}

#endif