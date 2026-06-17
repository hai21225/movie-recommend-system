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

            // Kiểm tra xem mật khẩu nhập lại có khớp nhau không
            if (Password != ConfirmPassword)
            {
                ErrorMessage = "Mật khẩu xác nhận không trùng khớp!";
                return Page();
            }

            // Đóng gói dữ liệu DTO gửi sang API Đăng ký của nhóm bạn
            var registerData = new { Username = Username, Password = Password };

            // Gọi hàm POST sang cổng Backend API
            var isSuccess = await _apiService.PostAsync("/api/auth/register", registerData);

            if (isSuccess)
            {
                // Nếu đăng ký thành công -> Chuyển hướng người dùng sang trang Login luôn để họ đăng nhập
                return RedirectToPage("Login");
            }
            else
            {
                ErrorMessage = "Tên tài khoản đã tồn tại hoặc hệ thống gặp sự cố!";
                return Page();
            }
        }
    }
}