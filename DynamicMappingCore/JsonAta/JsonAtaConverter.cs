using DynamicMappingCore.Models;
using System.Collections.Generic;
using System.Linq;

namespace DynamicMappingCore.JsonAta
{
    public class JsonAtaConverter
    {
        public const string Array = "Array";

        /// <summary>
        ///  json field which need to translate to array datatype
        /// </summary>
        public List<string> ArrayFields; 

        /// <summary>
        /// Convert JsonMetaData List to dictionary 
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, object> ConvertToDictionary(List<JsonMetaData> JsonMetaDataList)
        {
            Dictionary<string, object> dictObjectList = new Dictionary<string, object>();
            ArrayFields = new List<string>();

            JsonMetaDataList.ForEach(x =>
            {
                if (x.ComponentType != Array)
                    dictObjectList.TryAdd(x.Target, x.Expression != null ? x.Expression : x.Source);
                else
                    ArrayFields.Add(x.Target.Split(".").LastOrDefault());
            });

            return dictObjectList;
        }

    }
}
