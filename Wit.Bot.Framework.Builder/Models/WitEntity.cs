using Newtonsoft.Json;

namespace Wit.Bot.Framework.Builder.Models
{
    public class WitEntity
    {
        [JsonProperty(PropertyName = "confidence")]
        public float Confidence { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "value")]
        public string Value { get; set; }

        [JsonProperty(PropertyName = "suggested")]
        public bool Suggested { get; set; }
    }
}
