using Jolt.Net;
using JsonFlatten;
using JUST;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;

namespace DynamicMapping
{
    class Program
    {
        static void Main(string[] args)
        {
            //read input json file
            string shopifyJson = File.ReadAllText("Shopify.json");

            JObject jObj = JObject.Parse(shopifyJson);
            var sourceFlattenDictionary = new Dictionary<string, object>(jObj.Flatten());

            //read output json file
            string d365Json = File.ReadAllText("D365.json");

            jObj = JObject.Parse(d365Json);
            var targetFlattenDictionary = new Dictionary<string, object>(jObj.Flatten());

            //read transform json file
            string transformJson = File.ReadAllText("Transform.json");
            string outputjson = new JsonTransformer().Transform(transformJson, shopifyJson);
            Console.WriteLine(outputjson);

            //JOLT WORK

            var spec = GetJson("JoltTransform.json");
            var input = GetJson("Shopify.json");
            
            Chainr chainr = Chainr.FromSpec(spec);
            var transformedOutput = chainr.Transform(input);

            Console.WriteLine(transformedOutput.ToString());
        }

        static JToken GetJson(string path)
        {
            return JToken.Parse(File.ReadAllText(path));
        }
    }
    public class CustomFunction
    {
        public static string VariantValue(dynamic product,dynamic variant)
        {
            string value = "";
            string actualValue = "";
            foreach (var option in product.options)
            {
                if (product.options[0].name == option.name)
                    value = variant.option1;
                else if (product.options[1].name == option.name)
                    value = variant.option2;
                else if (product.options[2].name == option.name)
                    value = variant.option3;
                switch (option.name)
                {
                    case "Size":
                        actualValue = value;
                        break;
                    case "Color":
                        actualValue = value;
                        break;
                    case "Style":
                        actualValue = value;
                        break;
                    case "Config":
                        actualValue = value;
                        break;
                }
            }
            return actualValue;
        }
    }
}
