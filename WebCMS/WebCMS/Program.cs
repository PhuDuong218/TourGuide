using Microsoft.AspNetCore.Authentication.Cookies;
using WebCMS.Services;

var builder = WebApplication.CreateBuilder(args);

// ======================
// 1. LẤY URL API
// ======================
var apiUrl = builder.Configuration["ApiSettings:BaseUrl"];
if (string.IsNullOrEmpty(apiUrl))
{
    apiUrl = "https://gzm4vrwg-7054.asse.devtunnels.ms/api/";
}

// ======================
// 2. ADD SERVICES
// ======================
builder.Services.AddControllersWithViews();

// ======================
// 🔥 HTTP CLIENT SERVICES
// ======================

// POI
builder.Services.AddHttpClient<IPOIService, POIService>(client =>
{
    client.BaseAddress = new Uri(apiUrl);
});

// VisitHistory (interface)
builder.Services.AddHttpClient<IVisitHistoryService, VisitHistoryService>(client =>
{
    client.BaseAddress = new Uri(apiUrl.Replace("/api/", "/"));
});

// VisitHistory (direct - optional)
builder.Services.AddHttpClient<VisitHistoryService>(client =>
{
    client.BaseAddress = new Uri(apiUrl);
});

// Translation
builder.Services.AddHttpClient<TranslationService>(client =>
{
    client.BaseAddress = new Uri(apiUrl);
});

// OwnerRequest
builder.Services.AddHttpClient<OwnerRequestService>(client =>
{
    client.BaseAddress = new Uri(apiUrl);
});

// ======================
// ✅ SESSION
// ======================
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ======================
// ✅ AUTH
// ======================
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });

builder.Services.AddAuthorization();

// ======================
// OTHER
// ======================
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// ======================
// 3. PIPELINE
// ======================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// ⚠️ THỨ TỰ RẤT QUAN TRỌNG
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// ROUTE
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();