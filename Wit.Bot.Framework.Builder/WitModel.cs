using Microsoft.Bot.Builder.Internals.Fibers;
using System;
using System.Collections.Generic;

namespace Wit.Bot.Framework.Builder
{
    public enum WitApiVersion
    {
        Standard
    }

    public interface IWitModel
    {
        string AuthToken { get; }


        Uri UriBase { get; }

        WitApiVersion ApiVersion { get; }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
    [Serializable]
    public class WitModelAttribute : Attribute, IWitModel, IEquatable<IWitModel>
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
        /// Construct the wit model information.
        /// </summary>
        /// <param name="modelID">The wit model ID.</param>
        /// <param name="subscriptionKey">The wit subscription key.</param>
        /// <param name="apiVersion">The wit API version.</param>
        public WitModelAttribute(string authToken, WitApiVersion apiVersion = WitApiVersion.Standard)
        {
            SetField.NotNull(out this.authToken, nameof(authToken), authToken);
            this.apiVersion = apiVersion;
            this.uriBase = WitEndpoints[this.apiVersion];
        }

        public bool Equals(IWitModel other)
        {
            return other != null
                && object.Equals(this.AuthToken, other.AuthToken)
                && object.Equals(this.ApiVersion, other.ApiVersion)
                && object.Equals(this.UriBase, other.UriBase)
                ;
        }

        public override bool Equals(object other)
        {
            return this.Equals(other as IWitModel);
        }

        public override int GetHashCode()
        {
            return AuthToken.GetHashCode()
                ^ UriBase.GetHashCode()
                ^ ApiVersion.GetHashCode();
        }
    }
}
