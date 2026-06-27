using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;
using Web.Services;

namespace Web.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly ApiService _apiService;

        [BindProperty]
        public string Username { get; set; } = string.Empty;

        [BindProperty]
        public string Password { get; set; } = string.Empty;

        [BindProperty]
        public string ConfirmPassword { get; set; } = string.Empty;

        public string ErrorMessage { get; set; } = string.Empty;

        public RegisterModel(ApiService apiService)
        {
            _apiService = apiService;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            if (Password != ConfirmPassword)
            {
                ErrorMessage = "Mật khẩu xác nhận không trùng khớp!";
                return Page();
            }

            var registerData = new
            {
                Username,
                Password,
                ConfirmPassword
            };

            var isSuccess = await _apiService.PostAsync(
                "/api/Auth/register",
                registerData);

            if (isSuccess)
            {
                return RedirectToPage("/Login");
            }

            ErrorMessage = "Tên tài khoản đã tồn tại hoặc đăng ký thất bại!";
            return Page();
        }
    }
}