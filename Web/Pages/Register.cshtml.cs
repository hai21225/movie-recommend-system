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
            {
                return Page();
            }

            if (Password != ConfirmPassword)
            {
                ErrorMessage = "Mật khẩu xác nhận không trùng khớp!";
                return Page();
            }

            // ĐÃ SỬA: Đóng gói đúng 3 trường dữ liệu khớp 100% với RegisterDto của Backend
            var registerData = new { 
                Username = Username, 
                Password = Password, 
                ConfirmPassword = ConfirmPassword 
            };

            // Gọi hàm POST sang cổng Backend API
            var isSuccess = await _apiService.PostAsync("/api/auth/register", registerData);

            if (isSuccess)
            {
                // Đăng ký thành công vào DB -> Chuyển hướng sang Login
                return RedirectToPage("/Login");
            }
            else
            {
                ErrorMessage = "Tên tài khoản đã tồn tại hoặc đăng ký thất bại!";
                return Page();
            }
        }
    }
}