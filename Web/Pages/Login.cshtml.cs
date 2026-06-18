using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http; // Thư viện dùng cho Session
using System.Threading.Tasks;
using Web.Services;

namespace Web.Pages
{
    public class LoginModel : PageModel
    {
        private readonly ApiService _apiService;

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
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Đóng gói dữ liệu khớp với LoginDto của Backend
            var loginData = new { Username = Username, Password = Password };

            // Gửi dữ liệu qua API để check DB
            var isSuccess = await _apiService.PostAsync("/api/auth/login", loginData);

            if (isSuccess)
            {
                // ĐÃ THÊM: Lưu Username vào Session để giữ đăng nhập cho chức năng Bình luận
                HttpContext.Session.SetString("Username", Username);
                
                // Thành công -> Quay về trang chủ
                return RedirectToPage("Index");
            }
            else
            {
                ErrorMessage = "Tài khoản hoặc mật khẩu không chính xác!";
                return Page();
            }
        }
    }
}