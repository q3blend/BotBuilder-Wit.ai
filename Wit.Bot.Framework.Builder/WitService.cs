using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Rest;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wit.Bot.Framework.Builder.Models;

namespace Wit.Bot.Framework.Builder
{
    public sealed class WitRequest
    {
        public readonly string Query;

        public readonly string SessionId;

        public readonly string Context;

        public WitRequest(string query,
            string sessionId, string context = default(string))
        {
            this.Query = query;
            this.SessionId = sessionId;
            this.Context = context;
        }

        /// <summary>
        /// Build the Uri for issuing the request for the specified wit model.
        /// </summary>
        /// <param name="model"> The wit model.</param>
        /// <returns> The request Uri.</returns>
        public Uri BuildUri(IWitModel model)
        {
            if (SessionId == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "session id");
            }

            var queryParameters = new List<string>();
            queryParameters.Add($"session_id={Uri.EscapeDataString(SessionId)}");
            queryParameters.Add($"q={Uri.EscapeDataString(Query)}");
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

    public interface IWitService
    {
        HttpRequestMessage BuildRequest(WitRequest witRequest);

        Task<WitResult> QueryAsync(HttpRequestMessage request, CancellationToken token);
    }

    [Serializable]
    public sealed class WitService : IWitService
    {
        private readonly IWitModel model;

        /// <summary>
        /// Construct the wit service using the model information.
        /// </summary>
        /// <param name="model">The wit model information.</param>
        public WitService(IWitModel model)
        {
            SetField.NotNull(out this.model, nameof(model), model);
        }

        public HttpRequestMessage BuildRequest(WitRequest witRequest)
        {
            Uri uri = witRequest.BuildUri(this.model);
            return this.BuildRequest(uri, witRequest);
        }

        private HttpRequestMessage BuildRequest(Uri uri, WitRequest witRequest)
        {

            if (this.model.AuthToken == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "Authorization Token");
            }

            var request = new HttpRequestMessage()
            {
                RequestUri = uri,
                Method = HttpMethod.Post,
            };

            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", this.model.AuthToken);
            request.Content = new StringContent(witRequest.Context, Encoding.UTF8, "application/json");

            return request;
        }

        public async Task<WitResult> QueryAsync(HttpRequestMessage request, CancellationToken token)
        {
            string json = string.Empty;

            using (var client = new HttpClient())
            {
                var task = client.SendAsync(request)
                    .ContinueWith((taskwithmsg) =>
                    {
                        var response = taskwithmsg.Result;

                        var jsonTask = response.Content.ReadAsStringAsync();
                        jsonTask.Wait();
                        json = jsonTask.Result;
                    });
                task.Wait();
            }

            try
            {
                var result = JsonConvert.DeserializeObject<WitResult>(json);
                //might need to add wiring based on action type here?
                return result;
            }
            catch (JsonException ex)
            {
                throw new ArgumentException("Unable to deserialize the Wit response.", ex);
            }
        }
    }

    public static partial class Extensions
    {
        public static async Task<WitResult> QueryAsync(this IWitService service, string text, string sessionId, string context, CancellationToken token)
        {
            var request = service.BuildRequest(new WitRequest(text, sessionId, context));
            return await service.QueryAsync(request, token);
        }
    }
}

