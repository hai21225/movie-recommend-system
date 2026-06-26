using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Web.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;

        public ApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<T?> GetAsync<T>(string endpoint)
        {
            try
            {
                var response = await _httpClient.GetAsync(endpoint);

                if (!response.IsSuccessStatusCode)
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[LỖI GET] {endpoint} | {response.StatusCode} | {errorMsg}");
                    return default;
                }

                var content = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrWhiteSpace(content))
                    return default;

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                return JsonSerializer.Deserialize<T>(content, options);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LỖI GET] {endpoint} | {ex.Message}");
                return default;
            }
        }

        public async Task<bool> PostAsync(string endpoint, object data)
        {
            try
            {
                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(data),
                    Encoding.UTF8,
                    "application/json");

                Console.WriteLine($"[DEBUG POST] {_httpClient.BaseAddress}{endpoint}");

                var response = await _httpClient.PostAsync(endpoint, jsonContent);

                if (!response.IsSuccessStatusCode)
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[LỖI POST] {endpoint} | {response.StatusCode} | {errorMsg}");
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LỖI POST] {endpoint} | {ex.Message}");
                return false;
            }
        }

        public async Task<T?> PostWithResultAsync<T>(string endpoint, object data)
        {
            try
            {
                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(data),
                    Encoding.UTF8,
                    "application/json");

                Console.WriteLine($"[DEBUG POST RESULT] {_httpClient.BaseAddress}{endpoint}");

                var response = await _httpClient.PostAsync(endpoint, jsonContent);

                if (!response.IsSuccessStatusCode)
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[LỖI POST RESULT] {endpoint} | {response.StatusCode} | {errorMsg}");
                    return default;
                }

                var content = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrWhiteSpace(content))
                    return default;

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                return JsonSerializer.Deserialize<T>(content, options);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LỖI POST RESULT] {endpoint} | {ex.Message}");
                return default;
            }
        }

        public async Task<bool> SetLikeStatus(int userId, string showId, bool isLiked)
        {
            var data = new
            {
                UserId = userId,
                ShowId = showId,
                IsLiked = isLiked
            };

            return await PostAsync("/like", data);
        }

        public async Task<bool> CheckIsLiked(int userId, string showId)
        {
            try
            {
                var data = new
                {
                    UserId = userId,
                    ShowId = showId
                };

                var request = new HttpRequestMessage(HttpMethod.Get, "/is-liked")
                {
                    Content = new StringContent(
                        JsonSerializer.Serialize(data),
                        Encoding.UTF8,
                        "application/json")
                };

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[LỖI CHECK LIKE] /is-liked | {response.StatusCode} | {errorMsg}");
                    return false;
                }

                var content = await response.Content.ReadAsStringAsync();

                return bool.TryParse(content, out bool result) && result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LỖI CHECK LIKE] {ex.Message}");
                return false;
            }
        }

        public async Task<T?> GetUserHistory<T>(int userId)
        {
            return await GetAsync<T>($"/history/{userId}");
        }
    }
}