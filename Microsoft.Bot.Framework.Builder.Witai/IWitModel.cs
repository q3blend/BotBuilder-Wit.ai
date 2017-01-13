using System;

namespace Microsoft.Bot.Framework.Builder.Witai
{
    /// <summary>
    /// Wit Api Version
    /// </summary>
    public enum WitApiVersion
    {
        Standard
    }

    public interface IWitModel
    {
        /// <summary>
        /// The Wit model authorization token
        /// </summary>
        string AuthToken { get; }

        /// <summary>
        /// The base Uri for accessing Wit.
        /// </summary>
        Uri UriBase { get; }

        /// <summary>
        /// Wit Api Version
        /// </summary>
        WitApiVersion ApiVersion { get; }
    }
}
