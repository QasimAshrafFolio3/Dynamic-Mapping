using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace JsonFlattener
{
    public class ComAxJsonFlattener
    {
        List<ComAxJsonModel> jsonModelList;
        public ComAxJsonFlattener()
        {
            jsonModelList = new List<ComAxJsonModel>();
        }

        public List<ComAxJsonModel> CollectFields(JToken jToken, bool includeObject = false, bool includeArray = false)
        {

            switch (jToken.Type)
            {
                case JTokenType.Object:
                    foreach (var child in jToken.Children<JProperty>())
                    {
                        if (includeObject && !string.IsNullOrWhiteSpace(jToken.Path) && !jsonModelList.Any(x => x.Name == jToken.Path))
                        {
                            jsonModelList.Add(new ComAxJsonModel()
                            {
                                Name = jToken.Path,
                                Type = jToken.Type.ToString()
                            });
                        }
                        CollectFields(child,includeObject,includeArray);

                    }
                    break;
                case JTokenType.Array:
                    foreach (var child in jToken.Children())
                    {
                        if (includeArray && !string.IsNullOrWhiteSpace(jToken.Path) && !jsonModelList.Any(x => x.Name == jToken.Path))
                        {
                            jsonModelList.Add(new ComAxJsonModel()
                            {
                                Name = jToken.Path,
                                Type = jToken.Type.ToString()
                            });
                        }
                        CollectFields(child,includeObject,includeArray);
                    }
                    break;
                case JTokenType.Property:
                    CollectFields(((JProperty)jToken).Value,includeObject,includeArray);
                    break;
                default:
                    jsonModelList.Add(new ComAxJsonModel()
                    {
                        Name = jToken.Path,
                        Type = jToken.Type.ToString()
                    });
                    break;
            }
            return jsonModelList;
        }
    }
}
