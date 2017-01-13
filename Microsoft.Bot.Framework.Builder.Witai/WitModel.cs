using Microsoft.Bot.Builder.Internals.Fibers;
using System;
using System.Collections.Generic;

namespace Microsoft.Bot.Framework.Builder.Witai
{
    /// <summary>
    /// The Wit model information.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
    [Serializable]
    public class WitModelAttribute : Attribute, IWitModel
    {
        private readonly string authToken;

        public string AuthToken => authToken;

        private readonly Uri uriBase;

        public Uri UriBase => uriBase;

        private readonly WitApiVersion apiVersion;

        public WitApiVersion ApiVersion => apiVersion;

        public static readonly IReadOnlyDictionary<WitApiVersion, Uri> WitEndpoints = new Dictionary<WitApiVersion, Uri>()
        {
            {WitApiVersion.Standard, new Uri("https://api.wit.ai/converse?v=20160526")}
        };

        /// <summary>
        /// Construct the Wit model information.
        /// </summary>
        /// <param name="authToken">The Wit model authorization token.</param>
        /// <param name="apiVersion">The wit API version.</param>
        public WitModelAttribute(string authToken, WitApiVersion apiVersion = WitApiVersion.Standard)
        {
            SetField.NotNull(out this.authToken, nameof(authToken), authToken);
            this.apiVersion = apiVersion;
            SetField.NotNull(out this.uriBase, nameof(uriBase), WitEndpoints[this.apiVersion]);
        }
    }
}
