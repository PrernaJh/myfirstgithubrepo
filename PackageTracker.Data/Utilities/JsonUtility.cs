using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace PackageTracker.Data.Utilities
{
    public static class JsonUtility<T>
    {
        private static JsonSerializerOptions options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            IgnoreNullValues = true,
            WriteIndented = true,
        };

        public static string Serialize(T input)
        {
            return JsonSerializer.Serialize(input, options);
        }

        public static T Deserialize(string json)
        {
            return JsonSerializer.Deserialize<T>(json.ToString(), options);
        }

        public static IEnumerable<string> SerializeList(IEnumerable<T> inputList)
        {
            var response = new ConcurrentBag<string>();
            Parallel.ForEach(inputList, x =>
            {
                response.Add(JsonSerializer.Serialize(x, options));
            }); 
            return response;
        }

        public static IEnumerable<T> DeserializeList(IEnumerable<string> inputList)
        {
            var response = new ConcurrentBag<T>();
            Parallel.ForEach(inputList, x =>
            {
                response.Add(JsonSerializer.Deserialize<T>(x, options));
            });
            return response;
        }
    }
}