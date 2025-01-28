using Newtonsoft.Json;

namespace QMS.Models
{
    public class AssignFilter
    {

        [JsonProperty("ahtMin")]
        public string AhtMin { get; set; }

        [JsonProperty("ahtMax")]
        public string AhtMax { get; set; }

        [JsonProperty("disposition")]
        public string Disposition { get; set; }

        [JsonProperty("Process")]
        public string Process { get; set; }

        [JsonProperty("SubProcess")]
        public string SubProcess { get; set; }
    }
}
