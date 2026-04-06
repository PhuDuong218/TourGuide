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
// Đăng ký POIService thông qua Interface
builder.Services.AddHttpClient<IPOIService, POIService>(client =>
{
    client.BaseAddress = new Uri(apiUrl);
});

// Đăng ký TranslationService trực tiếp
builder.Services.AddHttpClient<TranslationService>(client =>
{
    client.BaseAddress = new Uri(apiUrl);
});

var app = builder.Build();

// 🔥 5. PIPELINE (Cấu hình luồng xử lý)
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Lưu ý: Nếu gặp lỗi SSL khi gọi API http (5015), bạn có thể tạm tắt dòng này để test
app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

// 🔥 6. ROUTE
app.MapControllerRoute(
    name: "default",
    // Theo yêu cầu của bạn: Chạy Account/Login trước khi vào trang chủ
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();