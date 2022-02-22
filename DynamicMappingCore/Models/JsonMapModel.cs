using System.Collections.Generic;

namespace DynamicMappingCore.Models
{
    public class JsonMapModel
    {
        public string Title { get; set; }
        public List<FieldsAndType> FieldsAndType { get; set; }
        public JsonMapModel()
        {
            FieldsAndType = new List<FieldsAndType>();
        }

    }
    public class FieldsAndType
    {
        public string Field { get; set; }
        public string Type { get; set; }
    }
}
