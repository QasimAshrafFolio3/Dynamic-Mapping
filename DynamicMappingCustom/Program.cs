using Newtonsoft.Json.Linq;
using System.IO;

namespace DynamicMappingCustom
{
    class Program
    {
        static void Main(string[] args)
        {
            string input = File.ReadAllText("Input.js");
            JObject jObject = JObject.Parse(input);
            //JToken jToken = null;
            var result = jObject.SelectToken("Phone").Type;

        }
    }
}
