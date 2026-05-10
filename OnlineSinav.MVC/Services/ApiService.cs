using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace OnlineSinav.MVC.Services
{
    public class ApiService
    {
        private readonly HttpClient _client;
        private readonly ITokenStorage _tokenStorage;

        public ApiService(IConfiguration configuration, ITokenStorage tokenStorage)
        {
            _tokenStorage = tokenStorage;
            _client = new HttpClient
            {
                BaseAddress = new Uri(configuration["ApiSettings:BaseUrl"])
            };
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        private void AddTokenToHeader()
        {
            var token = _tokenStorage.GetToken();
            if (!string.IsNullOrEmpty(token))
            {
                _client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<T?> GetAsync<T>(string url)
        {
            AddTokenToHeader();
            var response = await _client.GetAsync(url);
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(json);
        }

        public async Task<T?> PostAsync<T>(string url, object data)
        {
            AddTokenToHeader();
            var jsonData = JsonConvert.SerializeObject(data);
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            var response = await _client.PostAsync(url, content);
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(json);
        }

        public async Task<T?> PutAsync<T>(string url, object data)
        {
            AddTokenToHeader();
            var jsonData = JsonConvert.SerializeObject(data);
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            var response = await _client.PutAsync(url, content);
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(json);
        }

        public async Task<T?> DeleteAsync<T>(string url)
        {
            AddTokenToHeader();
            var response = await _client.DeleteAsync(url);
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}