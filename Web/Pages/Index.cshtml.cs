using Microsoft.AspNetCore.Http;
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

        public IndexModel(
            ApiService apiService,
            IWebHostEnvironment env,
            ILogger<IndexModel> logger)
        {
            _apiService = apiService;
            _env = env;
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null || userId <= 0)
            {
                return RedirectToPage("/Login");
            }

            var preferences = await _apiService.GetAsync<List<int>>(
                $"/api/Genre/user-preference/{userId.Value}");

            if (preferences == null || !preferences.Any())
            {
                return RedirectToPage("/ChooseGenres");
            }

            try
            {
                AllGenres = await _apiService.GetAsync<List<WebGenresDto>>(
                    "/api/Genre") ?? new();

                HomeFeeds = await _apiService.GetAsync<List<WebContentDto>>(
                    $"/api/Recommendation/genres/{userId.Value}?limit=20") ?? new();

                int index = 1;

                foreach (var movie in HomeFeeds)
                {
                    if (movie.Id <= 0)
                    {
                        movie.Id = index;
                    }

                    if (string.IsNullOrWhiteSpace(movie.ShowId))
                    {
                        movie.ShowId = $"movie-{index}";
                    }

                    if (string.IsNullOrWhiteSpace(movie.Type) &&
                        !string.IsNullOrWhiteSpace(movie.ContentType))
                    {
                        movie.Type = movie.ContentType;
                    }

                    if (string.IsNullOrWhiteSpace(movie.Type))
                    {
                        movie.Type = "Movie";
                    }

                    if (string.IsNullOrWhiteSpace(movie.ReleaseYear))
                    {
                        movie.ReleaseYear = "N/A";
                    }

                    if (string.IsNullOrWhiteSpace(movie.Duration))
                    {
                        movie.Duration = "N/A";
                    }

                    index++;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Không kết nối được API Backend, sẽ chuyển sang đọc CSV.");
            }

            if (!HomeFeeds.Any())
            {
                LoadRealDataFromCsv();
            }

            if (!HomeFeeds.Any())
            {
                ErrorMessage = "Không tìm thấy dữ liệu phim.";
            }

            IsLoading = false;

            return Page();
        }

        private void LoadRealDataFromCsv()
        {
            try
            {
                HomeFeeds.Clear();

                if (!AllGenres.Any())
                {
                    AllGenres.Clear();
                }

                string filePath = Path.Combine(
                    _env.WebRootPath,
                    "data",
                    "netflix_titles.csv");

                if (!System.IO.File.Exists(filePath))
                    return;

                var lines = System.IO.File.ReadAllLines(filePath);

                if (lines.Length <= 1)
                    return;

                int currentId = 1;
                HashSet<string> uniqueGenres = new();

                foreach (var line in lines.Skip(1).Take(80))
                {
                    var parts = ParseCsvLine(line);

                    if (parts.Count < 12)
                        continue;

                    string showId = parts[0].Trim(' ', '"');
                    string type = parts[1].Trim(' ', '"');
                    string title = parts[2].Trim(' ', '"');
                    string releaseYear = parts[7].Trim(' ', '"');
                    string duration = parts[9].Trim(' ', '"');
                    string genres = parts[10].Trim(' ', '"');

                    HomeFeeds.Add(new WebContentDto
                    {
                        Id = currentId++,
                        ShowId = showId,
                        Title = title,
                        Type = string.IsNullOrWhiteSpace(type) ? "Movie" : type,
                        ReleaseYear = string.IsNullOrWhiteSpace(releaseYear) ? "N/A" : releaseYear,
                        Duration = string.IsNullOrWhiteSpace(duration) ? "N/A" : duration
                    });

                    foreach (var g in genres.Split(','))
                    {
                        var clean = g.Replace("\"", "").Trim();

                        if (!string.IsNullOrWhiteSpace(clean))
                        {
                            uniqueGenres.Add(clean);
                        }
                    }
                }

                if (!AllGenres.Any())
                {
                    int genreId = 1;

                    AllGenres = uniqueGenres
                        .OrderBy(x => x)
                        .Take(20)
                        .Select(x => new WebGenresDto
                        {
                            Id = genreId++,
                            Name = x
                        })
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi đọc netflix_titles.csv");
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