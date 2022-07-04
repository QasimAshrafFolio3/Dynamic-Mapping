using DynamicMappingCore.Models;
using System.Collections.Generic;

namespace DynamicMappingCore.Db
{
    public static class Database
    {
        public static Dictionary<string, JsonModel> InMemJsonModeDb = new Dictionary<string, JsonModel>();
        public static Dictionary<string, List<JsonMetaData>> InMemJsonMetaDataDb = new Dictionary<string, List<JsonMetaData>>();
        
    }
}
