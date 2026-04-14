using Microsoft.EntityFrameworkCore;
using TourGuideServer.Data;
using TourGuideServer.Services;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// 1. Connection string
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=.;Database=TourGuideDB;Trusted_Connection=True;TrustServerCertificate=True;";

// 2. Cấu hình DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// 3. Đăng ký các Services
builder.Services.AddScoped<POIService>();

// 4. Cấu hình Controllers và JSON (QUAN TRỌNG)
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // 🔥 Đảm bảo trả về camelCase (poiId, name...) để App MAUI dễ đọc
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;

        // 🔥 Bỏ qua lỗi vòng lặp khi dùng .Include() trong POIController
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;

        // Cho phép đọc số từ chuỗi nếu cần
        options.JsonSerializerOptions.NumberHandling = JsonNumberHandling.AllowReadingFromString;
    });

// 5. CORS — Cho phép tất cả các nguồn gọi API
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

// 6. Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 7. Cấu hình Pipeline xử lý
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 🔥 QUAN TRỌNG: Thứ tự các Middleware này không được sai
app.UseStaticFiles();
app.UseRouting();

// Bật CORS trước khi Map Controllers
app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();