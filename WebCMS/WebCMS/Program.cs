using Microsoft.AspNetCore.Authentication.Cookies;
using WebCMS.Models;
using WebCMS.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. LẤY URL API
var apiUrl = builder.Configuration["ApiSettings:BaseUrl"];
if (string.IsNullOrEmpty(apiUrl))
{
    // Dự phòng nếu appsettings.json trống
    apiUrl = "https://gzm4vrwg-7054.asse.devtunnels.ms/api/";
}

// 2. ADD SERVICES
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<TranslationService>();

// Cấu hình Session (QUAN TRỌNG)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddHttpContextAccessor();

// Cấu hình HttpClient dùng chung apiUrl
builder.Services.AddHttpClient<IVisitHistoryService, VisitHistoryService>(client => {
    client.BaseAddress = new Uri(apiUrl.Replace("/api/", "/")); // Trỏ về root server
});

builder.Services.AddHttpClient<IPOIService, POIService>(client => {
    client.BaseAddress = new Uri(apiUrl);
});

// AUTH
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options => {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// 4. PIPELINE
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// THỨ TỰ PHẢI ĐÚNG: Session -> Auth -> Authorization
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();