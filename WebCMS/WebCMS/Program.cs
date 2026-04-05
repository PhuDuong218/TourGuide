using WebCMS.Models;
using WebCMS.Services;

var builder = WebApplication.CreateBuilder(args);

// 🔥 1. LẤY URL API TỪ appsettings.json
var apiUrl = builder.Configuration["ApiSettings:BaseUrl"];

// 🔥 2. CHỐNG NULL (QUAN TRỌNG)
if (string.IsNullOrEmpty(apiUrl))
{
    throw new Exception("❌ Lỗi: ApiSettings:BaseUrl chưa được cấu hình trong appsettings.json");
}

// 🔥 3. ADD MVC
builder.Services.AddControllersWithViews();

// 🔥 4. CẤU HÌNH HTTPCLIENT GỌI API
builder.Services.AddHttpClient<IPOIService, POIService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:5016/api/");
});

builder.Services.AddHttpClient<TranslationService>(client =>
{
    client.BaseAddress = new Uri(apiUrl);
});

var app = builder.Build();

// 🔥 5. PIPELINE
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

// 🔥 6. ROUTE
app.MapControllerRoute(
    name: "default",
    // Sửa chỗ này: Ban đầu vào sẽ chạy Account/Login thay vì Home/Index
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();