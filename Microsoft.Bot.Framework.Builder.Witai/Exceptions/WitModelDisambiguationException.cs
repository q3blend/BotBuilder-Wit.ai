using System;

namespace Microsoft.Bot.Framework.Builder.Exceptions
{
    /// <summary>
    /// Exception type thrown when an unable to disambiguate between different Wit models
    /// </summary>
    [Serializable]
    internal class WitModelDisambiguationException : Exception
    {
        public WitModelDisambiguationException(string message) : base(message)
        {
        }
    }
}
