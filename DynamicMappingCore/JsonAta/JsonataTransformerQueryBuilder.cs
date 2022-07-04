using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace DynamicMappingCore.JsonAta
{
    public class JsonataTransformerQueryBuilder
    {
        public static Regex TernaryOperatorRegex = new Regex(@"{*}*?{*}*:{*}*");
        private static string kvpFormat = "\"{0}\":{1},";

        private List<string> ArraysForConversion = new List<string>();
        public JsonataTransformerQueryBuilder(List<string> arraysForConversion)
        {
            ArraysForConversion = arraysForConversion;
        }

        public string BuildQuery(JObject jObject)
        {
            StringBuilder jsonataQuery = new StringBuilder();
            foreach (KeyValuePair<string, JToken> jToken in jObject)
            {
                string tempValue = string.Empty;

                // Lookup for nested objects
                if (jToken.Value.Type == JTokenType.Object)
                {
                    tempValue = BuildQuery(JObject.Parse(jToken.Value.ToString()));

                    if (ArraysForConversion.Contains(jToken.Key))
                        tempValue = $"[${tempValue}]";
                }
                else
                {
                    //Check for loop
                    if (jToken.Value.ToString().Contains("$map"))
                        tempValue = MapFunctionQuery(jToken.Value.ToString());
                    else if (TernaryOperatorRegex.IsMatch(jToken.Value.ToString()) || jToken.Value.ToString().Contains("="))
                        tempValue = jToken.Value.ToString();
                    else
                        tempValue = jToken.Value.ToString().Replace("\"", "").Replace("::", "\"");
                }

                jsonataQuery.Append(CreateKeyValueJson(jToken.Key, tempValue));
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
            int endIndex = json.LastIndexOf("}})");
            string jsonKvp = json.Substring(startIndex, endIndex - startIndex);

            foreach (var lines in jsonKvp.Split(",", System.StringSplitOptions.RemoveEmptyEntries))
            {
                var kvp = lines.Split(" : ");
                mapFunctionQuery.Append(CreateKeyValueJson(kvp[0], kvp[1]));
            }

            //Cleaning 
            mapFunctionQuery.Replace("\"\"", "\"");
            
            // Convert jsonata Double Bracket to Single Bracket
            mapFunctionQuery = new StringBuilder(json.Replace(jsonKvp, mapFunctionQuery.ToString()));
            return mapFunctionQuery.ToString();
        }

        
        private string CreateKeyValueJson(string Key, string value)
        {
            return string.Format(kvpFormat, Key, value);
        }
    }
}