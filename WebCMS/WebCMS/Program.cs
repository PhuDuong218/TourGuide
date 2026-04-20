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
builder.Services.AddControllers();

// 🔥 4. CẤU HÌNH HTTPCLIENT GỌI API (ĐĂNG KÝ CÁC DỊCH VỤ)
// Đăng ký StatsService
builder.Services.AddHttpClient<StatsService>(client =>
{
    client.BaseAddress = new Uri(apiUrl);
});
// Đăng ký POIService
builder.Services.AddHttpClient<IPOIService, POIService>(client =>
{
    client.BaseAddress = new Uri(apiUrl);
});

// Đăng ký TranslationService
builder.Services.AddHttpClient<TranslationService>(client =>
{
    client.BaseAddress = new Uri(apiUrl);
});

// ✅ Đăng ký OwnerRequestService (MỚI)
builder.Services.AddHttpClient<OwnerRequestService>(client =>
{
    client.BaseAddress = new Uri(apiUrl);
});

// ✅ Đăng ký VisitHistoryService (MỚI)
builder.Services.AddHttpClient<VisitHistoryService>(client =>
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

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapControllers();

// 🔥 6. ROUTE
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();