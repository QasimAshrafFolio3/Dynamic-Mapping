using JsonFlattener;
using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace CustomJsonFlatten
{
    class Program
    {
        static void Main(string[] args)
        {
            var input = File.ReadAllText("Input.js");
            var jToken = JToken.Parse(input);
            var allFields = new ComAxJsonFlattener().CollectFields(jToken);
            foreach (var singleField in allFields)
            {
                Console.WriteLine(singleField.Name + " -- " + singleField.Type);
            }

        }
    }
}