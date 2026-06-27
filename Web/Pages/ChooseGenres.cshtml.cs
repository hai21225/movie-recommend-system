using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.Models;
using Web.Services;

namespace Web.Pages
{
    public class ChooseGenresModel : PageModel
    {
        private readonly ApiService _apiService;

        public ChooseGenresModel(ApiService apiService)
        {
            _apiService = apiService;
        }

        public List<WebGenresDto> Genres { get; set; } = new();

        [BindProperty]
        public List<int> SelectedGenreIds { get; set; } = new();

        public string ErrorMessage { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null || userId <= 0)
            {
                return RedirectToPage("/Login");
            }

            var preferences = await _apiService.GetAsync<List<int>>(
                $"/api/Genre/user-preference/{userId.Value}");

            if (preferences != null && preferences.Any())
            {
                return RedirectToPage("/Index");
            }

            Genres = await _apiService.GetAsync<List<WebGenresDto>>(
                "/api/Genre") ?? new();

            if (!Genres.Any())
            {
                ErrorMessage = "Không tải được danh sách thể loại.";
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null || userId <= 0)
            {
                return RedirectToPage("/Login");
            }

            if (SelectedGenreIds == null || SelectedGenreIds.Count != 3)
            {
                ErrorMessage = "Bạn cần chọn đúng 3 thể loại.";

                Genres = await _apiService.GetAsync<List<WebGenresDto>>(
                    "/api/Genre") ?? new();

                return Page();
            }

            var request = new
            {
                UserId = userId.Value,
                GenreIds = SelectedGenreIds
            };

            var success = await _apiService.PostAsync(
                "/api/Genre/save-preference",
                request);

            if (success)
            {
                return RedirectToPage("/Index");
            }

            ErrorMessage = "Lưu thể loại thất bại.";

            Genres = await _apiService.GetAsync<List<WebGenresDto>>(
                "/api/Genre") ?? new();

            return Page();
        }
    }
}