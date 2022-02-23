namespace DynamicMappingCore.Models
{
    public class JsonModel
    {
        public string URL { get; set; }
        public ConnectionType ConnectionType { get; set; }
        public string Json { get; set; }
    }

    public enum ConnectionType
    {
        source = 1,
        destination = 2
    }
}
