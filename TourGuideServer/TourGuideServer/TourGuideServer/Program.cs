using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using TourGuideServer.Data;
using TourGuideServer.Services;

var builder = WebApplication.CreateBuilder(args);

// 🔥 1. Connection string
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=.;Database=TourGuideDB;Trusted_Connection=True;TrustServerCertificate=True;";

// 🔥 2. Đăng ký DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// 🔥 3. Đăng ký Service
builder.Services.AddScoped<POIService>();

// 🔥 4. Add Controllers + Fix vòng lặp JSON
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

// 🔥 5. CORS (cho phép gọi từ Web)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    policy.WithOrigins("https://localhost:7001", "http://localhost:7001") // Thêm cổng 7001 của WebCMS vào đây
          .AllowAnyMethod()
          .AllowAnyHeader());
});

// 🔥 6. Swagger (test API)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 🔥 7. Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Nếu bạn dùng HTTPS thì bật lại
app.UseHttpsRedirection();

app.UseCors("AllowAll");

// 🔥 8. Map controller
app.MapControllers();

// 🔥 9. Run
app.Run();