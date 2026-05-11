using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

namespace OnlineSinav.MVC.Services
{
    public class ApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ITokenStorage _tokenStorage;
        private readonly string _baseUrl;

        public ApiService(IConfiguration configuration, ITokenStorage tokenStorage, IHttpClientFactory httpClientFactory)
        {
            _tokenStorage = tokenStorage;
            _httpClientFactory = httpClientFactory;
            _baseUrl = configuration["ApiSettings:BaseUrl"]!;
        }

        // Her istek için token'lı, redirect'te auth header'ı koruyacak şekilde fresh client al
        private HttpClient CreateClient()
        {
            var client = _httpClientFactory.CreateClient("API");
            client.BaseAddress = new Uri(_baseUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var token = _tokenStorage.GetToken();
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return client;
        }

        public async Task<T?> GetAsync<T>(string url)
        {
            var client = CreateClient();
            var response = await client.GetAsync(url);
            return JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync());
        }

        public async Task<string> GetRawAsync(string url)
        {
            var client = CreateClient();
            var response = await client.GetAsync(url);
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<T?> PostAsync<T>(string url, object? data)
        {
            var client = CreateClient();
            var body = data != null ? JsonConvert.SerializeObject(data) : "{}";
            var content = new StringContent(body, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(url, content);
            return JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync());
        }

        public async Task<T?> PutAsync<T>(string url, object data)
        {
            var client = CreateClient();
            var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
            var response = await client.PutAsync(url, content);
            return JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync());
        }

        public async Task<T?> DeleteAsync<T>(string url)
        {
            var client = CreateClient();
            var response = await client.DeleteAsync(url);
            return JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync());
        }
    }
}