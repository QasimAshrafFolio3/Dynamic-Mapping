using System.Collections.Generic;

namespace DynamicMappingCore.Models
{
    // Used for UI/View Purpose 
    public class JsonMapModel
    {
        public string Title { get; set; }
        public List<FieldsAndType> FieldsAndType { get; set; }
        public ConnectionType ConnectionType { get; set; }
        public JsonMapModel()
        {
            FieldsAndType = new List<FieldsAndType>();
        }
    }

    // Used for DropDown Purpose 
    public class FieldsAndType
    {
        public string Field { get; set; }
        public string Type { get; set; }
    }
}
