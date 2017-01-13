using Newtonsoft.Json;
using System.Collections.Generic;

namespace Microsoft.Bot.Framework.Builder.Witai.Models
{
    public class WitResult
    {
        public string Type { get; set; }

        [JsonProperty(PropertyName = "msg")]
        public string Message { get; set; }

        public float Confidence { get; set; }

        public string Action { get; set; }

        public string[] QuickReplies { get; set; }

        public Dictionary<string, IList<WitEntity>> Entities { get; set; }
    }
}
