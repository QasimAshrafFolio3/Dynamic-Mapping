namespace DynamicMappingCore.Models
{
    /// <summary>
    /// This Model is responsible to store URL and json
    /// </summary>
    public class JsonModel
    {
        public string URL { get; set; }
        public ConnectionType ConnectionType { get; set; }
        public string Json { get; set; }
    }
}
