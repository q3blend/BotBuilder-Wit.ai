using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Rest;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Microsoft.Bot.Framework.Builder.Witai
{
    /// <summary>
    /// Object that contains all the possible parameters to build a Wit request.
    /// </summary>
    public sealed class WitRequest : IWitRequest
    {
        public string Query { get; }

        public string SessionId => sessionId;

        private string sessionId;

        public string Context => context;

        private string context;

        public WitRequest(string query,
            string sessionId, string context = "{}")
        {
            this.Query = query;
            SetField.NotNull(out this.sessionId, nameof(sessionId), sessionId);
            SetField.NotNull(out this.context, nameof(context), context);
        }

        /// <summary>
        /// Build the Uri for issuing the request for the specified wit model.
        /// </summary>
        /// <param name="model"> The wit model.</param>
        /// <returns> The request Uri.</returns>
        public HttpRequestMessage BuildRequest(IWitModel model)
        {
            if (model.AuthToken == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "Authorization Token");
            }

            var request = new HttpRequestMessage()
            {
                RequestUri = this.BuildUri(model),
                Method = HttpMethod.Post,
            };

            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", model.AuthToken);
            request.Content = new StringContent(this.Context, Encoding.UTF8, "application/json");

            return request;
        }

        
        private Uri BuildUri(IWitModel model)
        {
            if (SessionId == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "session id");
            }

            var queryParameters = new List<string>();
            queryParameters.Add($"session_id={Uri.EscapeDataString(SessionId)}");

            if (!string.IsNullOrEmpty(Query))
            {
                queryParameters.Add($"q={Uri.EscapeDataString(Query)}");
            }

            UriBuilder builder;

            switch (model.ApiVersion)
            {
                case WitApiVersion.Standard:
                    builder = new UriBuilder(model.UriBase);
                    break;
                default:
                    throw new ArgumentException($"{model.ApiVersion} is not a valid Wit api version.");
            }

            builder.Query = string.Join("&", queryParameters);
            return builder.Uri;
        }
    }

}
