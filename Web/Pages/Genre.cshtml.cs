using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Web.Models;
using Web.Services;

namespace Web.Pages
{
    public class GenreModel : PageModel
    {
        private readonly ApiService _apiService;

        public GenreModel(ApiService apiService)
        {
            _apiService = apiService;
        }

        public List<WebContentDto> Movies { get; set; } = new();
        public string GenreName { get; set; } = "Thể loại";
        public string ErrorMessage { get; set; } = string.Empty;

        public async Task OnGetAsync(int id)
        {
            try
            {
                var genres = await _apiService.GetAsync<List<WebGenresDto>>("/api/genre");
                if (genres != null)
                {
                    var targetGenre = genres.FirstOrDefault(g => g.Id == id);
                    if (targetGenre != null)
                    {
                        // Thêm toán tử ?? để chặn giá trị null, sửa dứt điểm lỗi CS8601
                        GenreName = targetGenre.Name ?? "Thể loại"; 
                    }
                }

                int userId = HttpContext.Session.GetInt32("UserId") ?? 1;
                
                // Gọi đồng bộ về cụm dữ liệu phân tích hệ thống
                var contentsResponse = await _apiService.GetAsync<List<WebContentDto>>($"/api/recommendation/home-feed/{userId}");
                
                if (contentsResponse != null && contentsResponse.Any())
                {
                    Movies = contentsResponse;
                }
                else
                {
                    ErrorMessage = "Hiện tại danh mục phim thuộc thể loại này đang được cập nhật.";
                }
            }
            catch (System.Exception ex)
            {
                ErrorMessage = "Lỗi kết nối máy chủ: " + ex.Message;
            }
        }
    }
}