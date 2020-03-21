using System.Text.Json;

namespace CBS.Siren.Utilities
{
    public static class JsonExtensions
    {
        public static string SerializeObjectDataToJsonString(this object objData)
        {
            JsonSerializerOptions documentOptions = new JsonSerializerOptions();
            documentOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            return JsonSerializer.Serialize(objData, documentOptions);
        }
    }
}
