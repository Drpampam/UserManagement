using Newtonsoft.Json;
using System.Net.Http.Headers;
using UserManagement.Core.Interfaces;

namespace UserManagement.Infrastructure.ExternalServices
{
    public class HttpClientService : IHttpClientService
    {
        private readonly IHttpClientFactory _clientFactory;
        public HttpClientService(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public async Task<TRes> GetRequestAsync<TRes>(string baseUrl, string url, string token = "") where TRes : class
        {
            var client = CreateClient(baseUrl, token);
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            return await GetResponseResultAsync<TRes>(client, request);
        }

        public async Task<TRes> PostRequestAsync<TReq, TRes>(string baseUrl, string url, TReq requestModel, string token = "")
            where TRes : class
            where TReq : class
        {
            var client = CreateClient(baseUrl, token);
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(JsonConvert.SerializeObject(requestModel), null, "application/json")
            };
            return await GetResponseResultAsync<TRes>(client, request);
        }

        private async Task<TRes> GetResponseResultAsync<TRes>(HttpClient client, HttpRequestMessage request) where TRes : class
        {
            var response = await client.SendAsync(request);
            var responseString = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<TRes>(responseString);
            return response.IsSuccessStatusCode ? result! : throw new ArgumentException(response?.ReasonPhrase);
        }

        private HttpClient CreateClient(string baseUrl, string token)
        {
            var client = _clientFactory.CreateClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.BaseAddress = new Uri(baseUrl);
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return client;
        }
    }
}
