using System.Net.Http;

namespace Microsoft.Bot.Framework.Builder.Witai
{
    /// <summary>
    /// Object that contains all the possible parameters to build a Wit request.
    /// </summary>
    public interface IWitRequest
    {
        string Query { get; }

        string SessionId { get; }

        string Context { get; }

        /// <summary>
        /// Build the Uri for issuing the request for the specified wit model.
        /// </summary>
        /// <param name="model"> The wit model.</param>
        /// <returns> The request Uri.</returns>
        HttpRequestMessage BuildRequest(IWitModel model);
    }
}
