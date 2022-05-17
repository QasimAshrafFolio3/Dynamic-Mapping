using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DynamicMappingCore.Models
{
    // Used to Convert Model into Jsonata Query.
    public class JsonMetaData
    {
        public string Source { get; set; }
        public string Expression { get; set; }
        public string Target { get; set; }
        public string ComponentType { get; set; }
    }
}
