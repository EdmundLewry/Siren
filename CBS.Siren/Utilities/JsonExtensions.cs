using System.Text.Json;

namespace CBS.Siren.Utilities
{
    public static class JsonExtensions
    {
        public static string SerializeToJson(this object objData)
        {
            JsonSerializerOptions documentOptions = new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
            return JsonSerializer.Serialize(objData, documentOptions);
        }

        public static T DeserializeJson<T>(this string content)
        {
            JsonSerializerOptions documentOptions = new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            };
            return JsonSerializer.Deserialize<T>(content, documentOptions);
        }
    }
}
