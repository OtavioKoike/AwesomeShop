using AwesomeShop.Services.Orders.Core.Clients;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace AwesomeShop.Services.Orders.Infrastructure.Clients
{
    public class GenericHttpClient: IGenericHttpClient
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public GenericHttpClient(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<string> GetAsync (Uri url)
        {
            using(var httpClient = _httpClientFactory.CreateClient())
            {
                using(var httpResponse = await httpClient.GetAsync(url))
                {
                    return await ValidationResponse(httpResponse);
                }
            }
        }

        private async Task<string> ValidationResponse(HttpResponseMessage httpResponse)
        {
            var response = await httpResponse.Content.ReadAsStringAsync();

            switch (httpResponse.StatusCode)
            {
                case HttpStatusCode.OK:
                case HttpStatusCode.Created:
                case HttpStatusCode.NoContent:
                    return response;

                case HttpStatusCode.BadRequest:
                case HttpStatusCode.Unauthorized:
                case HttpStatusCode.NotFound:
                    return null;

                default:
                    return null;
            }
        }
    }
}
