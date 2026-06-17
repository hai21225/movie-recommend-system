using Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Đăng ký HttpClient kết nối tới Backend API
builder.Services.AddHttpClient<ApiService>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5281"); 
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// 💥 BẮT BUỘC PHẢI THÊM DÒNG NÀY VÀO ĐỂ BẬT CSS/JS RIÊNG
app.UseStaticFiles(); 

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();