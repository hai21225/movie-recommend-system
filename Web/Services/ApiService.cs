using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Web.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;

        public ApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            // ĐÃ SỬA: Tăng thời gian chờ lên 30 giây để Database có thời gian khởi động
            _httpClient.Timeout = TimeSpan.FromSeconds(30); 
        }

        public async Task<T?> GetAsync<T>(string endpoint)
        {
            try
            {
                var response = await _httpClient.GetAsync(endpoint);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<T>();
                }
                return default;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LỖI GET] {ex.Message}");
                return default; 
            }
        }

        public async Task<bool> PostAsync<T>(string endpoint, T data)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(endpoint, data);
                
                // IN LỖI ĐỎ RA MÀN HÌNH NẾU BACKEND TỪ CHỐI
                if (!response.IsSuccessStatusCode)
                {
                    var errorDetails = await response.Content.ReadAsStringAsync();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"\n[BACKEND TỪ CHỐI] Lỗi {response.StatusCode} tại {endpoint}");
                    Console.WriteLine($"Chi tiết: {errorDetails}\n");
                    Console.ResetColor();
                }
                
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                // IN LỖI VÀNG RA MÀN HÌNH NẾU BỊ RỚT MẠNG HOẶC TIMEOUT
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"\n[LỖI KẾT NỐI API] Không thể gửi tới {endpoint}");
                Console.WriteLine($"Lý do: {ex.Message}\n");
                Console.ResetColor();
                return false;
            }
        }
    }
}