using DynamicMappingCore.Db;
using DynamicMappingCore.JsonAta;
using DynamicMappingCore.Models;
using Jsonata.Net.Native;
using JsonFlatten;
using JsonFlattener;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace DynamicMappingCore.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

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
            return Json(Database.InMemJsonMetaDataDb.Where(x => !string.IsNullOrWhiteSpace(x.Key)).Select(x => x.Value));
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
            Database.InMemJsonModeDb.TryAdd(jsonModel.URL, jsonModel);
            return RedirectToAction("ShowJson");
        }

        public IActionResult ShowJson()
        {
            List<JsonModel> jsonModelList = new List<JsonModel>();
            foreach (var item in Database.InMemJsonModeDb)
                jsonModelList.Add(new JsonModel { URL = item.Key, ConnectionType = item.Value.ConnectionType, Json = item.Value.Json });

            return View(jsonModelList);
        }

        // Map Json Using Custom Flattener
        public IActionResult MapJsonCustom()
        {
            List<JsonMapModel> jsonMapModelList = new List<JsonMapModel>();

            //Fetch all json from 
            foreach (var item in Database.InMemJsonModeDb)
            {
                JsonMapModel tempJsonMapModel = new JsonMapModel();

                // Convert url to Title
                tempJsonMapModel.Title = item.Key.Substring(item.Key.LastIndexOf('/') + 1);

                List<FieldsAndType> fieldsAndTypesList = new List<FieldsAndType>();

                // Parse json and extract Info
                var jsonModel = item.Value;
                tempJsonMapModel.ConnectionType = jsonModel.ConnectionType;
                JObject jObj = JObject.Parse(IsArray(jsonModel.Json) ? JArray.Parse(jsonModel.Json).FirstOrDefault().ToString() : jsonModel.Json);

                //Conver json to Flattener Model
                var jsonCustomFormatter = new ComAxJsonFlattener().CollectFields(JToken.Parse(jObj.ToString()), false, true);

                foreach (var fields in jsonCustomFormatter)
                {
                    // cleaning simple flatten pattern like removing array brackets. etc.
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

        [HttpPost]
        public IActionResult SaveMappingJsonAtaWithQuery(List<JsonMetaData> JsonMetaDataList, string jsonataQuery)
        {
            Database.InMemJsonMetaDataDb.Clear();

            currentJsonAtaQuery = jsonataQuery;

            JsonAtaConverter jsonAtaConverter = new JsonAtaConverter();
            var outputJoBject = jsonAtaConverter.ConvertToDictionary(JsonMetaDataList).Unflatten();
            var arrayForConversion = jsonAtaConverter.ArrayFields;

            if (outputJoBject != null)
            {
                //Convert Dictionary to proper json ata query
                currentUnflattenJsonAta = new JsonataTransformerQueryBuilder(arrayForConversion).BuildQuery(outputJoBject);
                Database.InMemJsonMetaDataDb.Add(jsonataQuery.Trim(), JsonMetaDataList);
            }

            return RedirectToAction("ResultJson");
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

        public IActionResult ResultJson()
        {
            var source = Database.InMemJsonModeDb.Where(x => x.Value.ConnectionType == ConnectionType.source).FirstOrDefault().Value;
            ViewBag.Source = source.Json;

            ViewBag.JsonAtaQuery = currentJsonAtaQuery;
            ViewBag.UnflattenJsonAta = currentUnflattenJsonAta;

            if (currentUnflattenJsonAta != null)
            {
                JsonataQuery jsonataQuery = new JsonataQuery(currentUnflattenJsonAta);
                result = jsonataQuery.Eval(source.Json.ToString());
                ViewBag.Result = result;
            }
            return View();
        }
    }
}
