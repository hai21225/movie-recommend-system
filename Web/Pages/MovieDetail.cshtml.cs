using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Web.Models;
using Web.Services;

namespace Web.Pages
{
    // Tạo một class nhỏ để chứa cấu trúc Bình luận
    public class MovieComment
    {
        public int MovieId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class MovieDetailModel : PageModel
    {
        private readonly ApiService _apiService;
        private readonly IWebHostEnvironment _env;

        public WebContentDto? Movie { get; set; } 
        public List<WebContentDto> RecommendedMovies { get; set; } = new(); 
        public List<MovieComment> Comments { get; set; } = new();
        public string ErrorMessage { get; set; } = string.Empty;
        public bool IsLoggedIn { get; set; } = false;
        public string CurrentUser { get; set; } = "Khách";

        public MovieDetailModel(ApiService apiService, IWebHostEnvironment env)
        {
            _apiService = apiService;
            _env = env;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            if (id <= 0) return RedirectToPage("/Index");

            // Kiểm tra trạng thái đăng nhập (Thay "Username" bằng key Session thật của nhóm bạn nếu cần)
            var sessionUser = HttpContext.Session?.GetString("Username");
            if (!string.IsNullOrEmpty(sessionUser) || User.Identity?.IsAuthenticated == true)
            {
                IsLoggedIn = true;
                CurrentUser = sessionUser ?? User.Identity?.Name ?? "Thành viên Netflix";
            }

            try
            {
                Movie = await _apiService.GetAsync<WebContentDto>($"/api/content/{id}");
                if (Movie != null)
                {
                    RecommendedMovies = await _apiService.GetAsync<List<WebContentDto>>($"/api/recommend/similar-content/{id}") ?? new();
                }
            }
            catch { /* Lỗi API */ }

            if (Movie == null) LoadFallbackData(id);
            if (Movie == null) ErrorMessage = "Không tìm thấy dữ liệu bộ phim.";

            // Tải danh sách bình luận đã lưu của phim này
            LoadComments(id);

            return Page();
        }

        // HÀM MỚI: Xử lý khi người dùng Gửi bình luận từ Form
        public IActionResult OnPostAddComment(int id, string commentText)
        {
            if (string.IsNullOrWhiteSpace(commentText)) return RedirectToPage(new { id });

            // Lấy tên người dùng đang đăng nhập
            string username = HttpContext.Session?.GetString("Username") ?? User.Identity?.Name ?? "Thành viên Ẩn danh";

            var newComment = new MovieComment
            {
                MovieId = id,
                Username = username,
                Text = commentText,
                CreatedAt = DateTime.Now
            };

            SaveCommentToJson(newComment);

            // Tải lại trang sau khi bình luận xong
            return RedirectToPage(new { id });
        }

        // --- CÁC HÀM XỬ LÝ LƯU TRỮ LOCAL ---
        private void LoadComments(int movieId)
        {
            try
            {
                string filePath = Path.Combine(_env.WebRootPath, "data", "comments.json");
                if (System.IO.File.Exists(filePath))
                {
                    string json = System.IO.File.ReadAllText(filePath);
                    var allComments = JsonSerializer.Deserialize<List<MovieComment>>(json) ?? new List<MovieComment>();
                    // Chỉ lấy bình luận của phim hiện tại, sắp xếp mới nhất lên đầu
                    Comments = allComments.Where(c => c.MovieId == movieId).OrderByDescending(c => c.CreatedAt).ToList();
                }
            }
            catch { /* Bỏ qua nếu lỗi đọc file */ }
        }

        private void SaveCommentToJson(MovieComment comment)
        {
            try
            {
                string folderPath = Path.Combine(_env.WebRootPath, "data");
                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                string filePath = Path.Combine(folderPath, "comments.json");
                List<MovieComment> allComments = new();

                if (System.IO.File.Exists(filePath))
                {
                    string json = System.IO.File.ReadAllText(filePath);
                    allComments = JsonSerializer.Deserialize<List<MovieComment>>(json) ?? new List<MovieComment>();
                }

                allComments.Add(comment);
                System.IO.File.WriteAllText(filePath, JsonSerializer.Serialize(allComments, new JsonSerializerOptions { WriteIndented = true }));
            }
            catch { /* Bỏ qua nếu lỗi ghi file */ }
        }

        // (Giữ nguyên hàm LoadFallbackData và ParseCsvLine ở phía dưới như cũ)
        private void LoadFallbackData(int targetId)
        {
            var filePath = Path.Combine(_env.WebRootPath, "data", "netflix_titles.csv");
            if (!System.IO.File.Exists(filePath)) return;
            var lines = System.IO.File.ReadAllLines(filePath);
            if (lines.Length <= 1) return;
            int currentId = 1;
            var dataLines = lines.Skip(1).Take(100).ToList();
            List<WebContentDto> allMockMovies = new();
            foreach (var line in dataLines)
            {
                var parts = ParseCsvLine(line);
                if (parts.Count < 12) continue;
                allMockMovies.Add(new WebContentDto {
                    Id = currentId++, Type = parts[1].Trim(' ', '"'), Title = parts[2].Trim(' ', '"'),
                    ReleaseYear = string.IsNullOrWhiteSpace(parts[7]) ? "2022" : parts[7].Trim(' ', '"'),
                    Duration = string.IsNullOrWhiteSpace(parts[9]) ? "N/A" : parts[9].Trim(' ', '"')
                });
            }
            Movie = allMockMovies.FirstOrDefault(m => m.Id == targetId);
            if (Movie != null) RecommendedMovies = allMockMovies.Where(m => m.Id != targetId && m.Type == Movie.Type).Take(5).ToList();
        }

        private List<string> ParseCsvLine(string csvLine)
        {
            List<string> tokens = new(); bool inQuotes = false; string token = "";
            foreach (char c in csvLine) {
                if (c == '"') inQuotes = !inQuotes;
                else if (c == ',' && !inQuotes) { tokens.Add(token); token = ""; }
                else token += c;
            }
            tokens.Add(token); return tokens;
        }
    }
}