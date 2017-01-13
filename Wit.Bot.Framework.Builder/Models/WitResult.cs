using Newtonsoft.Json;
using System.Collections.Generic;

namespace Wit.Bot.Framework.Builder.Models
{
    public class WitResult
    {
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "msg")]
        public string message { get; set; }

        [JsonProperty(PropertyName = "confidence")]
        public float Confidence { get; set; }

        [JsonProperty(PropertyName = "action")]
        public string Action { get; set; }

        [JsonProperty(PropertyName = "quickreplies")]
        public string[] QuickReplies { get; set; } //need to verify it's a string

        [JsonProperty(PropertyName = "entities")]
        public Dictionary<string, IList<WitEntity>> Entities { get; set; }


    }
}
