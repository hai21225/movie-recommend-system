using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json.Serialization;
using Web.Services;

namespace Web.Pages
{
    public class HistoryModel : PageModel
    {
        private readonly ApiService _apiService;

        public HistoryModel(ApiService apiService)
        {
            _apiService = apiService;
        }

        public List<UserInteractionDto> LikedMovies { get; set; } = new();
        public string ErrorMessage { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null || userId <= 0)
            {
                return RedirectToPage("/Login");
            }

            var history = await _apiService.GetAsync<List<UserInteractionDto>>(
                $"/history/{userId.Value}") ?? new();

            LikedMovies = history
                .Where(x => x.Liked)
                .OrderByDescending(x => x.InteractedAt)
                .ToList();

            if (!LikedMovies.Any())
            {
                ErrorMessage = "Bạn chưa thích phim nào.";
            }

            return Page();
        }
    }

    public class UserInteractionDto
    {
        [JsonPropertyName("interactionId")]
        public int InteractionId { get; set; }

        [JsonPropertyName("userId")]
        public int UserId { get; set; }

        [JsonPropertyName("showId")]
        public string ShowId { get; set; } = string.Empty;

        [JsonPropertyName("isLiked")]
        public bool Liked { get; set; }

        [JsonPropertyName("interactedAt")]
        public DateTime InteractedAt { get; set; }
    }
}