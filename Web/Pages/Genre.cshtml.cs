using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Web.Models;
using Web.Services;

namespace Web.Pages
{
    public class GenreModel : PageModel
    {
        private readonly ApiService _apiService;
        private readonly IWebHostEnvironment _env;

        public GenreModel(ApiService apiService, IWebHostEnvironment env)
        {
            _apiService = apiService;
            _env = env;
        }

        public List<WebContentDto> Movies { get; set; } = new();
        public string GenreName { get; set; } = "Thể loại";
        public string ErrorMessage { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null || userId <= 0)
            {
                return RedirectToPage("/Login");
            }

            var genres = await _apiService.GetAsync<List<WebGenresDto>>("/api/Genre") ?? new();

            var targetGenre = genres.FirstOrDefault(g => g.Id == id);

            if (targetGenre == null)
            {
                ErrorMessage = "Không tìm thấy thể loại.";
                return Page();
            }

            GenreName = targetGenre.Name;

            LoadMoviesByGenreFromCsv(GenreName);

            if (!Movies.Any())
            {
                ErrorMessage = $"Hiện tại chưa có phim thuộc thể loại {GenreName}.";
            }

            return Page();
        }

        private void LoadMoviesByGenreFromCsv(string genreName)
        {
            string filePath = Path.Combine(_env.WebRootPath, "data", "netflix_titles.csv");

            if (!System.IO.File.Exists(filePath))
                return;

            var lines = System.IO.File.ReadAllLines(filePath);

            if (lines.Length <= 1)
                return;

            int currentId = 1;

            foreach (var line in lines.Skip(1))
            {
                var parts = ParseCsvLine(line);

                if (parts.Count < 12)
                    continue;

                string showId = parts[0].Trim(' ', '"');
                string type = parts[1].Trim(' ', '"');
                string title = parts[2].Trim(' ', '"');
                string releaseYear = parts[7].Trim(' ', '"');
                string duration = parts[9].Trim(' ', '"');
                string listedIn = parts[10].Trim(' ', '"');

                var genreList = listedIn
                    .Split(',')
                    .Select(g => g.Trim())
                    .ToList();

                bool match = genreList.Any(g =>
                    g.Equals(genreName, System.StringComparison.OrdinalIgnoreCase));

                if (!match)
                    continue;

                Movies.Add(new WebContentDto
                {
                    Id = currentId++,
                    ShowId = showId,
                    Title = title,
                    Type = string.IsNullOrWhiteSpace(type) ? "Movie" : type,
                    ReleaseYear = string.IsNullOrWhiteSpace(releaseYear) ? "N/A" : releaseYear,
                    Duration = string.IsNullOrWhiteSpace(duration) ? "N/A" : duration
                });

                if (Movies.Count >= 50)
                    break;
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