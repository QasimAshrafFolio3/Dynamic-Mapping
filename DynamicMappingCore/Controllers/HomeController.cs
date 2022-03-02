using DynamicMappingCore.Models;
using Jsonata.Net.Native;
using JsonFlatten;
using JsonFlattener;
using JUST;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace DynamicMappingCore.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public static Dictionary<string, JsonModel> InMemJsonModeDb = new Dictionary<string, JsonModel>();
        public static Dictionary<string, List<JsonMetaData>> InMemJsonMetaDataDb = new Dictionary<string, List<JsonMetaData>>();

        public static string result;
        public static string currentUnflattenJsonAta;
        public static string currentJsonAtaQuery;
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public JsonResult GetJsonMetaData()
        {
            return Json(InMemJsonMetaDataDb.Where(x => !string.IsNullOrWhiteSpace(x.Key)).Select(x => x.Value));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult AddJson()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SaveJson(JsonModel jsonModel)
        {
            InMemJsonModeDb.TryAdd(jsonModel.URL, jsonModel);
            return RedirectToAction("ShowJson");
        }

        public IActionResult ShowJson()
        {
            List<JsonModel> jsonModelList = new List<JsonModel>();
            foreach (var item in InMemJsonModeDb)
            {
                var jsonModel = item.Value;
                jsonModelList.Add(new JsonModel { URL = item.Key, ConnectionType = jsonModel.ConnectionType, Json = jsonModel.Json });
            }
            return View(jsonModelList);
        }

        public IActionResult MapJson()
        {
            List<JsonMapModel> jsonMapModelList = new List<JsonMapModel>();

            //Fetch all json from 
            foreach (var item in InMemJsonModeDb)
            {
                JsonMapModel tempJsonMapModel = new JsonMapModel();

                // Convert url to Title
                tempJsonMapModel.Title = item.Key.Substring(item.Key.LastIndexOf('/') + 1);
                List<FieldsAndType> fieldsAndTypesList = new List<FieldsAndType>();
                // Parse json and extract Info

                var jsonModel = item.Value;
                JObject jObj = JObject.Parse(IsArray(jsonModel.Json) ? JArray.Parse(jsonModel.Json).FirstOrDefault().ToString() : jsonModel.Json);
                IDictionary<string, object> keyValuePairs = jObj.Flatten();

                foreach (var fields in keyValuePairs)
                {
                    // Remove and convert json into simple flatten pattern 
                    string cleanFieldName = Regex.Replace(fields.Key, @"[[0-9]+]", "");

                    if (!fieldsAndTypesList.Any(x => x.Field == cleanFieldName))
                    {
                        fieldsAndTypesList.Add(new FieldsAndType()
                        {
                            Field = cleanFieldName,
                            Type = fields.Value != null ? fields.Value.GetType().Name : typeof(string).Name
                        });
                    }


                }
                tempJsonMapModel.FieldsAndType = fieldsAndTypesList;
                jsonMapModelList.Add(tempJsonMapModel);
            }
            return View(jsonMapModelList);
        }

        // Map Json Using Custom Flattener
        public IActionResult MapJsonCustom()
        {
            List<JsonMapModel> jsonMapModelList = new List<JsonMapModel>();

            //Fetch all json from 
            foreach (var item in InMemJsonModeDb)
            {
                JsonMapModel tempJsonMapModel = new JsonMapModel();

                // Convert url to Title
                tempJsonMapModel.Title = item.Key.Substring(item.Key.LastIndexOf('/') + 1);
                List<FieldsAndType> fieldsAndTypesList = new List<FieldsAndType>();
                // Parse json and extract Info
                var jsonModel = item.Value;
                tempJsonMapModel.ConnectionType = jsonModel.ConnectionType;
                JObject jObj = JObject.Parse(IsArray(jsonModel.Json) ? JArray.Parse(jsonModel.Json).FirstOrDefault().ToString() : jsonModel.Json);
                var jsonCustomFormatter = new ComAxJsonFlattener().CollectFields(JToken.Parse(jObj.ToString()), false, true);

                foreach (var fields in jsonCustomFormatter)
                {
                    // Remove and convert json into simple flatten pattern 
                    string cleanFieldName = Regex.Replace(fields.Name, @"[[0-9]+]", "");

                    if (!fieldsAndTypesList.Any(x => x.Field == cleanFieldName))
                    {
                        fieldsAndTypesList.Add(new FieldsAndType()
                        {
                            Field = cleanFieldName,
                            Type = fields.Type != "Null" ? fields.Type : typeof(string).Name
                        });
                    }
                }
                tempJsonMapModel.FieldsAndType = fieldsAndTypesList;
                jsonMapModelList.Add(tempJsonMapModel);
            }
            return View(jsonMapModelList);
        }

        private string GetJTokenType(JToken value)
        {
            var type = typeof(string).Name;

            switch (value.Type)
            {
                case JTokenType.String: type = typeof(string).Name; break;
                case JTokenType.Integer: type = typeof(int).Name; break;
                case JTokenType.Boolean: type = typeof(bool).Name; break;
                case JTokenType.Date: type = typeof(DateTime).Name; break;
                case JTokenType.Object: type = typeof(object).Name; break;
                case JTokenType.Array: type = typeof(Array).Name; break;
            }
            return type;

        }


        // Just.net Library
        [HttpPost]
        public IActionResult SaveMappingJustNet(Dictionary<string, string> keyValuePairs)
        {
            // to check
            string pattern = @"(\..*){2,}";
            Regex rg = new Regex(pattern);

            // Convert string values to object values to Unflatten.
            Dictionary<string, object> dictObjectList = new Dictionary<string, object>();

            for (int i = 0; i < keyValuePairs.Count; i++)
            {
                var tempkvp = keyValuePairs.ElementAt(i);
                //if (rg.IsMatch(tempkvp.Value))
                dictObjectList.TryAdd(tempkvp.Key, (object)tempkvp.Value);
            }

            var source = InMemJsonModeDb.Where(x => x.Value.ConnectionType == ConnectionType.source).FirstOrDefault().Value;
            result = "";

            //Unflatten
            JObject jObject = dictObjectList.Unflatten();

            //jObject.Values
            if (jObject != null)
            {
                // JUST.net
                if (IsArray(source.Json))
                {
                    foreach (var children in JArray.Parse(source.Json).Children())
                    {
                        result += new JsonTransformer().Transform(jObject.ToString(), children.ToString());
                    }
                }
                else
                {
                    result += new JsonTransformer().Transform(jObject.ToString(), source.Json);
                }
            }

            return RedirectToAction("ResultJson");
        }

        // Json Ata Library
        [HttpPost]
        public IActionResult SaveMappingJsonAta(Dictionary<string, string> keyValuePairs)
        {
            // Convert string values to object values to Unflatten.
            Dictionary<string, object> dictObjectList = keyValuePairs.ToDictionary(k => k.Key, k => (object)k.Value.ToString());
            var source = InMemJsonModeDb.Where(x => x.Value.ConnectionType == ConnectionType.source).FirstOrDefault().Value;
            result = "";

            //Unflatten
            JObject jObject = dictObjectList.Unflatten();

            //jObject.Values
            if (jObject != null)
            {

                //Pre Processing 
                string jsonataFinalQuery = CreateJsonAtaQuery(jObject);

                System.IO.File.WriteAllText(@"D:\\Result.jsonata", jsonataFinalQuery);

                if (IsArray(source.Json))
                {
                    foreach (var children in JArray.Parse(source.Json).Children())
                    {
                        JsonataQuery jsonataQuery = new JsonataQuery(jsonataFinalQuery);
                        result += jsonataQuery.Eval(children.ToString());
                    }
                }
                else
                {
                    // Json ata.
                    JsonataQuery jsonataQuery = new JsonataQuery(jsonataFinalQuery);
                    JToken data = JToken.Parse(source.Json);
                    result = jsonataQuery.Eval(data.ToString());
                }
            }

            return RedirectToAction("ResultJson");
        }

        [HttpPost]
        public IActionResult SaveMappingJsonAtaWithQuery(List<JsonMetaData> JsonMetaDataList, string jsonataQuery)
        {
            InMemJsonMetaDataDb.Clear();

            currentJsonAtaQuery = jsonataQuery;

            Dictionary<string, object> dictObjectList = new Dictionary<string, object>();
            JsonMetaDataList.ForEach(x => { dictObjectList.TryAdd(x.Target, x.Expression != null ? x.Expression : x.Source); });

            var outputJoBject = dictObjectList.Unflatten();
            currentUnflattenJsonAta = CreateJsonAtaQuery(outputJoBject);

            // Replace all ,} with };
            currentUnflattenJsonAta = currentUnflattenJsonAta.Replace(",}", "}");

            InMemJsonMetaDataDb.Add(jsonataQuery.Trim(), JsonMetaDataList);
            return RedirectToAction("ResultJson");
        }


        // Custom Mapper
        [HttpPost]
        public IActionResult SaveMappingCustom(Dictionary<string, string> keyValuePairs)
        {
            var source = InMemJsonModeDb.Where(x => x.Value.ConnectionType == ConnectionType.source).FirstOrDefault().Value;
            result = "";

            if (IsArray(source.Json))
            {
                foreach (var children in JArray.Parse(source.Json).Children())
                    ProcessNode(keyValuePairs, children.ToString());
            }
            else
            {
                ProcessNode(keyValuePairs, source.Json);
            }

            return RedirectToAction("ResultJson");
        }

        private string ProcessNode(Dictionary<string, string> keyValuePairs, string json)
        {
            //string tempJson = string.Empty;
            Dictionary<string, object> dictObjectList = new Dictionary<string, object>();

            JObject job = JObject.Parse(json);
            //Iterate over 
            foreach (var kvp in keyValuePairs)
            {
                var node = job.SelectToken(kvp.Value);
                if (node != null)
                {
                    //tempJson += string.Format("\"{0}\":\"{1}\",", kvp.Key, node);
                    dictObjectList.Add(kvp.Key, node);
                }
            }
            //result = JObject.Parse("{" + tempJson + "}").ToString();
            var outputJoBject = dictObjectList.Unflatten();
            result = outputJoBject.ToString();
            return result;
        }


        #region Private method
        private string CreateJsonAtaQuery(JObject jObject)
        {
            string tempJson = string.Empty;
            foreach (var x in jObject)
            {
                if (x.Value.Type == JTokenType.Object)
                    tempJson += string.Format("\"{0}\":{1},", x.Key, CreateJsonAtaQuery(JObject.Parse(x.Value.ToString())));
                else
                    tempJson += string.Format("\"{0}\":{1},", x.Key, x.Value.ToString().Replace("\"", "").Replace("::", "\""));
            }
            return "{" + tempJson + "}";
        }

        private bool IsArray(string source)
        {
            try
            {
                JArray.Parse(source);
                return true;
            }
            catch
            {
                return false;
            };
        }

        #endregion

        public IActionResult ResultJson()
        {
            var source = InMemJsonModeDb.Where(x => x.Value.ConnectionType == ConnectionType.source).FirstOrDefault().Value;
            ViewBag.Source = source.Json;

            ViewBag.JsonAtaQuery = currentJsonAtaQuery;
            ViewBag.UnflattenJsonAta = currentUnflattenJsonAta;

            JsonataQuery jsonataQuery = new JsonataQuery(currentUnflattenJsonAta);
            result = jsonataQuery.Eval(source.Json.ToString());
            ViewBag.Result = result;

            return View();
        }
    }
}
