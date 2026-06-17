using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Web.Models; // Sử dụng các DTO từ thư mục Models riêng biệt của bạn
using Web.Services;

namespace Web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ApiService _apiService;
        private readonly ILogger<IndexModel> _logger;

        // Danh sách phim và thể loại để đổ ra file HTML Index.cshtml
        public List<WebContentDto> HomeFeeds { get; set; } = new();
        public List<WebGenresDto> AllGenres { get; set; } = new();
        public string ErrorMessage { get; set; } = "";
        public bool IsLoading { get; set; } = true;

        // Giả lập ID người dùng hiện tại (cho tính năng gợi ý)
        private const int CurrentUserId = 1;

        public IndexModel(ApiService apiService, ILogger<IndexModel> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        // Hàm tự động chạy khi người dùng truy cập vào Trang Chủ
        public async Task OnGetAsync()
        {
            IsLoading = true;
            try
            {
                _logger.LogInformation("Đang tải dữ liệu trang chủ từ API...");
                
                // 1. Gọi API lấy dữ liệu danh sách phim gợi ý
                var resultFeed = await _apiService.GetAsync<List<WebContentDto>>("/api/recommend/home-feed?userId=" + CurrentUserId + "&count=20");
                if (resultFeed != null)
                {
                    HomeFeeds = resultFeed;
                }
                
                // 2. Gọi API lấy danh sách thể loại phim cho thanh Sidebar
                var resultGenres = await _apiService.GetAsync<List<WebGenresDto>>("/api/genre");
                if (resultGenres != null)
                {
                    AllGenres = resultGenres;
                }

                // Kiểm tra nếu cả hai đều không có dữ liệu -> Có thể Backend API đang bị tắt
                if (HomeFeeds.Count == 0 && AllGenres.Count == 0)
                {
                    ErrorMessage = "Không thể kết nối đến máy chủ API. Vui lòng đảm bảo Backend API đang chạy tại cổng 5281.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi xảy ra khi xử lý OnGetAsync");
                ErrorMessage = "Hệ thống giao diện đang gặp sự cố kết nối dữ liệu.";
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}