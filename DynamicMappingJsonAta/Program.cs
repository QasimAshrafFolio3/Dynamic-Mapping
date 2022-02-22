using Jsonata.Net.Native;
using Newtonsoft.Json.Linq;
using System.IO;

namespace DynamicMappingJsonAta
{
    class Program
    {
        static void Main(string[] args)
        {
            JToken? m_datasetJson = null;
            JObject? m_bindingsJson = null;

            string input = File.ReadAllText("Input.js");
            JsonataQuery query = new JsonataQuery(input);

            m_datasetJson = JToken.Parse(File.ReadAllText("Transfrom.js"));
            var res = query.Eval(m_datasetJson);
        }
    }
}
