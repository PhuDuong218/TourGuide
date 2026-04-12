using Microsoft.EntityFrameworkCore;
using TourGuideServer.Data;
using TourGuideServer.Services;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// 1. Connection string
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=.;Database=TourGuideDB;Trusted_Connection=True;TrustServerCertificate=True;";

// 2. DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// 3. Đăng ký các Services
builder.Services.AddScoped<POIService>();

// 4. Cấu hình Controllers và JSON (QUAN TRỌNG ĐỂ JAVASCRIPT ĐỌC ĐƯỢC)
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Đảm bảo tên biến trả về kiểu camelCase (ví dụ: qrId, poiName...)
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        // Tránh lỗi vòng lặp dữ liệu (Circular Reference)
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

// 5. CORS — Cho phép Web Admin (localhost:5016) gọi API
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin() // Cho phép tất cả các nguồn (Localhost 5016, Mobile...)
              .AllowAnyMethod()
              .AllowAnyHeader());
});

// 6. Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 7. Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Tắt HttpsRedirection nếu bạn đang chạy HTTP ở Local để tránh lỗi SSL
// app.UseHttpsRedirection();

// 🔥 QUAN TRỌNG: UseCors phải nằm TRƯỚC MapControllers
app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();