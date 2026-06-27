using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
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
    public class MovieComment
    {
        public string ShowId { get; set; } = string.Empty;
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
        public bool IsLiked { get; set; } = false;
        public string CurrentUser { get; set; } = "Khách";

        public MovieDetailModel(ApiService apiService, IWebHostEnvironment env)
        {
            _apiService = apiService;
            _env = env;
        }

        public async Task<IActionResult> OnGetAsync(string showId)
        {
            if (string.IsNullOrWhiteSpace(showId))
                return RedirectToPage("/Index");

            var sessionUser = HttpContext.Session?.GetString("Username");

            if (!string.IsNullOrEmpty(sessionUser) || User.Identity?.IsAuthenticated == true)
            {
                IsLoggedIn = true;
                CurrentUser = sessionUser ?? User.Identity?.Name ?? "Thành viên Netflix";
            }

            var userId = HttpContext.Session?.GetInt32("UserId") ?? 1;

            List<WebContentDto> homeFeed = new();

            try
            {
                homeFeed = await _apiService.GetAsync<List<WebContentDto>>(
                    $"/api/Recommendation/home-feed/{userId}?limit=100") ?? new();

                Movie = homeFeed.FirstOrDefault(m => m.ShowId == showId);

                if (Movie != null && !string.IsNullOrEmpty(Movie.ShowId))
                {
                    RecommendedMovies = await _apiService.GetAsync<List<WebContentDto>>(
                        $"/api/Recommendation/similar/{Movie.ShowId}?limit=10") ?? new();

                    if (!RecommendedMovies.Any())
                    {
                        RecommendedMovies = homeFeed
                            .Where(m => m.ShowId != showId && m.DisplayType == Movie.DisplayType)
                            .Take(10)
                            .ToList();
                    }
                }
            }
            catch
            {
            }

            if (Movie == null)
            {
                LoadFallbackData(showId);
            }

            if (Movie == null)
            {
                ErrorMessage = "Không tìm thấy dữ liệu bộ phim.";
            }
            else if (!string.IsNullOrEmpty(Movie.ShowId))
            {
                IsLiked = await _apiService.CheckIsLiked(userId, Movie.ShowId);
            }

            LoadComments(showId);

            return Page();
        }

        public IActionResult OnPostAddComment(string showId, string commentText)
        {
            if (string.IsNullOrWhiteSpace(commentText))
                return RedirectToPage(new { showId });

            string username =
                HttpContext.Session?.GetString("Username")
                ?? User.Identity?.Name
                ?? "Thành viên Ẩn danh";

            var newComment = new MovieComment
            {
                ShowId = showId,
                Username = username,
                Text = commentText,
                CreatedAt = DateTime.Now
            };

            SaveCommentToJson(newComment);

            return RedirectToPage(new { showId });
        }

        public async Task<IActionResult> OnPostToggleLikeAjax(string showId, bool currentIsLiked)
        {
            var userId = HttpContext.Session?.GetInt32("UserId");

            if (userId == null || userId <= 0)
            {
                return new JsonResult(new
                {
                    success = false,
                    message = "Bạn cần đăng nhập để thích phim."
                });
            }

            if (string.IsNullOrWhiteSpace(showId))
            {
                return new JsonResult(new
                {
                    success = false,
                    message = "Không tìm thấy mã phim."
                });
            }

            bool newStatus = !currentIsLiked;

            bool success = await _apiService.SetLikeStatus(
                userId.Value,
                showId,
                newStatus);

            return new JsonResult(new
            {
                success,
                newStatus
            });
        }

        private void LoadComments(string showId)
        {
            try
            {
                string filePath = Path.Combine(_env.WebRootPath, "data", "comments.json");

                if (!System.IO.File.Exists(filePath))
                    return;

                string json = System.IO.File.ReadAllText(filePath);

                var allComments =
                    JsonSerializer.Deserialize<List<MovieComment>>(json)
                    ?? new List<MovieComment>();

                Comments = allComments
                    .Where(c => c.ShowId == showId)
                    .OrderByDescending(c => c.CreatedAt)
                    .ToList();
            }
            catch
            {
            }
        }

        private void SaveCommentToJson(MovieComment comment)
        {
            try
            {
                string folderPath = Path.Combine(_env.WebRootPath, "data");

                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                string filePath = Path.Combine(folderPath, "comments.json");

                List<MovieComment> allComments = new();

                if (System.IO.File.Exists(filePath))
                {
                    string json = System.IO.File.ReadAllText(filePath);

                    allComments =
                        JsonSerializer.Deserialize<List<MovieComment>>(json)
                        ?? new List<MovieComment>();
                }

                allComments.Add(comment);

                System.IO.File.WriteAllText(
                    filePath,
                    JsonSerializer.Serialize(
                        allComments,
                        new JsonSerializerOptions
                        {
                            WriteIndented = true
                        }));
            }
            catch
            {
            }
        }

        private void LoadFallbackData(string targetShowId)
        {
            var filePath = Path.Combine(_env.WebRootPath, "data", "netflix_titles.csv");

            if (!System.IO.File.Exists(filePath))
                return;

            var lines = System.IO.File.ReadAllLines(filePath);

            if (lines.Length <= 1)
                return;

            int currentId = 1;
            var dataLines = lines.Skip(1).ToList();

            List<WebContentDto> allMockMovies = new();

            foreach (var line in dataLines)
            {
                var parts = ParseCsvLine(line);

                if (parts.Count < 12)
                    continue;

                allMockMovies.Add(new WebContentDto
                {
                    Id = currentId++,
                    ShowId = parts[0].Trim(' ', '"'),
                    Type = parts[1].Trim(' ', '"'),
                    Title = parts[2].Trim(' ', '"'),
                    ReleaseYear = string.IsNullOrWhiteSpace(parts[7]) ? "N/A" : parts[7].Trim(' ', '"'),
                    Duration = string.IsNullOrWhiteSpace(parts[9]) ? "N/A" : parts[9].Trim(' ', '"')
                });
            }

            Movie = allMockMovies.FirstOrDefault(m => m.ShowId == targetShowId);

            if (Movie != null)
            {
                RecommendedMovies = allMockMovies
                    .Where(m => m.ShowId != targetShowId && m.DisplayType == Movie.DisplayType)
                    .Take(10)
                    .ToList();
            }
        }

        private List<string> ParseCsvLine(string csvLine)
        {
            List<string> tokens = new();
            bool inQuotes = false;
            string token = "";

            foreach (char c in csvLine)
            {
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    tokens.Add(token);
                    token = "";
                }
                else
                {
                    token += c;
                }
            }

            tokens.Add(token);
            return tokens;
        }
    }
}