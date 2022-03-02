using Newtonsoft.Json.Linq;
using System.Text;

namespace DynamicMappingCore.JsonAta
{
    public class JsonataEngine
    {
        public string CreateJsonAtaQuery(JObject jObject)
        {
            StringBuilder jsonataQuery = new StringBuilder();
            foreach (var x in jObject)
            {
                //Check for loo
                if (x.Value.ToString().Contains("$map"))
                {
                    jsonataQuery.Append(CreateKeyValueJson(x.Key, MapFunctionQuery(x.Value.ToString())));
                }
                else
                {
                    jsonataQuery.Append(CreateKeyValueJson(x.Key,
                                        x.Value.Type == JTokenType.Object ?
                                        CreateJsonAtaQuery(JObject.Parse(x.Value.ToString()))  // If Object then explore further.
                                        : x.Value.ToString().Replace("\"", "").Replace("::", "\"")));
                }

            }

            //Post Processing
            jsonataQuery = new StringBuilder("{" + jsonataQuery + "}");

            // Replace all ,} with };
            jsonataQuery = jsonataQuery.Replace(",}", "}");

            return jsonataQuery.ToString();
        }

        private string MapFunctionQuery(string json)
        {
            StringBuilder mapFunctionQuery = new StringBuilder();

            int startIndex = json.IndexOf("{{") + 2;
            int endIndex =  json.LastIndexOf("}})");
            string kvpList = json.Substring(startIndex ,endIndex-startIndex);
            
            foreach (var lines in kvpList.Split(",",System.StringSplitOptions.RemoveEmptyEntries))
            {
                var kvp = lines.Split(":");
                mapFunctionQuery.Append(CreateKeyValueJson(kvp[0].Replace("\"", ""),kvp[1].Replace("\"", "")));
            }
            
            mapFunctionQuery = new StringBuilder(json.Replace(kvpList, mapFunctionQuery.ToString()));
            return mapFunctionQuery.ToString();
        }

        private static string kvpFormat = "\"{0}\":{1},";
        private string CreateKeyValueJson(string Key, string value)
        {
            return string.Format(kvpFormat, Key, value);
        }
    }
}