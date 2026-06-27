using Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// 1. KÍCH HOẠT BỘ NHỚ LƯU SESSION ĐĂNG NHẬP
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2); // Giữ đăng nhập trong 2 tiếng
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Đăng ký HttpClient kết nối tới Backend API (ĐÃ BỎ QUA LỖI SSL)
builder.Services.AddHttpClient<ApiService>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5281"); 
}).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStaticFiles(); 

app.UseRouting();

// 2. KÍCH HOẠT MIDDLEWARE SESSION
app.UseSession(); 

app.UseAuthorization();

app.MapRazorPages();

app.Run();