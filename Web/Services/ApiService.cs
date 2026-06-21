using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Web.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;

        // TIẾP NHẬN HTTPCLIENT TỪ PROGRAM.CS ĐÃ ĐƯỢC CẤU HÌNH ĐỊA CHỈ BASEURL = 5281
        public ApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<T?> GetAsync<T>(string endpoint)
        {
            try
            {
                // Chỉ truyền endpoint (VD: /api/genre), KHÔNG NỐI THÊM ĐỊA CHỈ NÀO KHÁC VÀO ĐÂY
                var response = await _httpClient.GetAsync(endpoint);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    if (!string.IsNullOrEmpty(content))
                    {
                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        return JsonSerializer.Deserialize<T>(content, options);
                    }
                }
                return default;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n[LỖI GET] {ex.Message}\n");
                return default;
            }
        }

        // ĐÃ CHỈNH SỬA LẠI HÀM POST ĐỂ GỬI ĐÚNG ĐỊA CHỈ VÀ TRẢ VỀ STATUS
        public async Task<bool> PostAsync(string endpoint, object data)
        {
            try
            {
                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(data),
                    Encoding.UTF8,
                    "application/json"
                );

                Console.WriteLine($"[DEBUG] Đang gửi POST tới: {_httpClient.BaseAddress}{endpoint}");

                // Chỉ truyền chuỗi endpoint (VD: /api/auth/register)
                var response = await _httpClient.PostAsync(endpoint, jsonContent);
                
                if(!response.IsSuccessStatusCode)
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[LỖI TỪ API] {errorMsg}");
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n[LỖI KẾT NỐI API POST] Không thể gửi tới {endpoint}\nLý do: {ex.Message}\n");
                return false;
            }
        }

        // Bổ sung hàm PostAsync trả về object nếu sau này Login cần lấy Token hoặc UserId
        public async Task<T?> PostWithResultAsync<T>(string endpoint, object data)
        {
            try
            {
                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(data),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PostAsync(endpoint, jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    return JsonSerializer.Deserialize<T>(content, options);
                }
                return default;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n[LỖI POST] {ex.Message}\n");
                return default;
            }
        }
    }
}