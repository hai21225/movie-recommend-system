using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web.Models;
using Web.Services;

namespace Web.Pages
{
    public class LoginModel : PageModel
    {
        private readonly ApiService _apiService;

        public LoginModel(ApiService apiService)
        {
            _apiService = apiService;
        }

        [BindProperty]
        public string Username { get; set; } = string.Empty;

        [BindProperty]
        public string Password { get; set; } = string.Empty;

        public string ErrorMessage { get; set; } = string.Empty;

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var loginData = new
            {
                Username,
                Password
            };

            var user = await _apiService.PostWithResultAsync<UserDto>(
                "/api/Auth/login",
                loginData);

            if (user == null || user.UserId <= 0)
            {
                ErrorMessage = "Tài khoản hoặc mật khẩu không chính xác!";
                return Page();
            }

            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("Username", user.UserName);

            var preferences = await _apiService.GetAsync<List<int>>(
                $"/api/Genre/user-preference/{user.UserId}");

            if (preferences == null || !preferences.Any())
            {
                return RedirectToPage("/ChooseGenres");
            }

            return RedirectToPage("/Index");
        }
    }
}