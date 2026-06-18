using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Web.Models;
using Web.Services;

namespace Web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ApiService _apiService;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<IndexModel> _logger;

        public List<WebContentDto> HomeFeeds { get; set; } = new();
        public List<WebGenresDto> AllGenres { get; set; } = new();
        public string ErrorMessage { get; set; } = string.Empty;
        public bool IsLoading { get; set; } = true;

        private const int CurrentUserId = 1;

        public IndexModel(ApiService apiService, IWebHostEnvironment env, ILogger<IndexModel> logger)
        {
            _apiService = apiService;
            _env = env;
            _logger = logger;
        }

        public async Task OnGetAsync()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                var resultGenres = await _apiService.GetAsync<List<WebGenresDto>>("/api/genre");
                if (resultGenres != null && resultGenres.Any()) AllGenres = resultGenres;

                var resultFeed = await _apiService.GetAsync<List<WebContentDto>>($"/api/recommend/home-feed?userId={CurrentUserId}&count=20");
                if (resultFeed != null && resultFeed.Any()) HomeFeeds = resultFeed;
            }
            catch (Exception ex)
            {
                // [ĐÃ FIX LỖI 1]: Sử dụng biến ex để ghi log hệ thống
                _logger.LogWarning(ex, "Không kết nối được API Backend, sẽ chuyển sang đọc file CSV.");
            }

            if (!AllGenres.Any() || !HomeFeeds.Any())
            {
                LoadRealDataFromCsv();
            }

            if (!AllGenres.Any() && !HomeFeeds.Any())
            {
                ErrorMessage = "Hệ thống Backend đang trống dữ liệu và không tìm thấy file dự phòng tại wwwroot/data/netflix_titles.csv";
            }

            IsLoading = false;
        }

        private void LoadRealDataFromCsv()
        {
            try
            {
                string filePath = Path.Combine(_env.WebRootPath, "data", "netflix_titles.csv");
                
                // [ĐÃ FIX LỖI 2]: Sử dụng System.IO.File để không bị nhầm lẫn với hàm File() của PageModel
                if (!System.IO.File.Exists(filePath)) return;

                var lines = System.IO.File.ReadAllLines(filePath);
                if (lines.Length <= 1) return;

                int currentId = 1;
                HashSet<string> uniqueGenres = new();
                var dataLines = lines.Skip(1).Take(80).ToList();

                foreach (var line in dataLines)
                {
                    var parts = ParseCsvLine(line);
                    if (parts.Count < 12) continue;

                    string type = parts[1].Trim();         
                    string title = parts[2].Trim();        
                    string releaseYearStr = parts[7].Trim(); 
                    string duration = parts[9].Trim();     
                    string genresStr = parts[10].Trim();   

                    int.TryParse(releaseYearStr, out int releaseYear);

                    HomeFeeds.Add(new WebContentDto
                    {
                        Id = currentId++,
                        Title = title,
                        Type = type,
                        // [ĐÃ FIX LỖI 3]: Đổi thành ReleaseYear chuẩn xác theo DTO của bạn
                        ReleaseYear = string.IsNullOrEmpty(releaseYearStr) ? "2022" : releaseYearStr,
                        Duration = string.IsNullOrEmpty(duration) ? "N/A" : duration
                    });

                    var rawGenres = genresStr.Split(',');
                    foreach (var g in rawGenres)
                    {
                        string cleanGenre = g.Replace("\"", "").Trim();
                        if (!string.IsNullOrEmpty(cleanGenre)) uniqueGenres.Add(cleanGenre);
                    }
                }

                int genreId = 1;
                AllGenres = uniqueGenres.Take(12).Select(g => new WebGenresDto { Id = genreId++, Name = g }).ToList();
            }
            catch (Exception ex)
            { 
                _logger.LogError(ex, "Lỗi phân tích file CSV."); 
            }
        }

        private List<string> ParseCsvLine(string csvLine)
        {
            List<string> tokens = new();
            bool inQuotes = false;
            string token = "";
            foreach (char c in csvLine)
            {
                if (c == '"') inQuotes = !inQuotes;
                else if (c == ',' && !inQuotes) { tokens.Add(token); token = ""; }
                else token += c;
            }
            tokens.Add(token);
            return tokens;
        }
    }
}