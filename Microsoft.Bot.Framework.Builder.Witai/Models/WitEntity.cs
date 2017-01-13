namespace Microsoft.Bot.Framework.Builder.Witai.Models
{
    public class WitEntity
    {
        public float Confidence { get; set; }
        
        public string Type { get; set; }
        
        public string Value { get; set; }
        
        public bool Suggested { get; set; }
    }
}
