using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Framework.Builder.Witai.Models;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Framework.Builder.Witai
{

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

        public Task<WitResult> QueryAsync(IWitRequest request, CancellationToken token)
        {
            var httpRequest = this.BuildRequest(request);
            return this.QueryAsync(httpRequest, token);
        }

        public HttpRequestMessage BuildRequest(IWitRequest witRequest)
        {
            return witRequest.BuildRequest(this.model);
        }

        private async Task<WitResult> QueryAsync(HttpRequestMessage request, CancellationToken token)
        {
            string json = string.Empty;

            using (var client = new HttpClient())
            {
                var response = await client.SendAsync(request);
                json = await response.Content.ReadAsStringAsync();
            }

            try
            {
                var result = JsonConvert.DeserializeObject<WitResult>(json);
                
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
            return await service.QueryAsync(new WitRequest(text, sessionId, context), token);
        }
    }
}

