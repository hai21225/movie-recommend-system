using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;
using Web.Services;

namespace Web.Pages
{
    public class LoginModel : PageModel
    {
        private readonly ApiService _apiService;

        // Bắt buộc dùng [BindProperty] để HTML có thể map dữ liệu qua thẻ asp-for
        [BindProperty]
        public string Username { get; set; } = string.Empty;

        [BindProperty]
        public string Password { get; set; } = string.Empty;

        public string ErrorMessage { get; set; } = string.Empty;

        public LoginModel(ApiService apiService)
        {
            _apiService = apiService;
        }

        public void OnGet()
        {
            // Chạy khi người dùng truy cập vào trang Đăng nhập
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Đóng gói dữ liệu thành object gửi sang Backend API
            var loginData = new { Username = Username, Password = Password };

            // Gửi dữ liệu qua API
            var isSuccess = await _apiService.PostAsync("/api/auth/login", loginData);

            if (isSuccess)
            {
                // Thành công -> Quay về trang chủ
                return RedirectToPage("Index");
            }
            else
            {
                // Thất bại -> Báo lỗi ra màn hình
                ErrorMessage = "Tài khoản hoặc mật khẩu không chính xác!";
                return Page();
            }
        }
    }
}