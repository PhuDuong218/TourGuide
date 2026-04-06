using Microsoft.EntityFrameworkCore;
using TourGuideServer.Data;
using TourGuideServer.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Connection string
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=.;Database=TourGuideDB;Trusted_Connection=True;TrustServerCertificate=True;";

// 2. DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// 3. Services
builder.Services.AddScoped<POIService>();

// 4. Controllers
builder.Services.AddControllers();

// 5. CORS — cho phép app mobile gọi API
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

// 6. Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "TourGuide API",
        Version = "v1",
        Description = "API cho ứng dụng hướng dẫn du lịch"
    });
});

var app = builder.Build();

// 7. Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Tắt HTTPS redirect để app mobile HTTP hoạt động được
// app.UseHttpsRedirection();

app.UseCors("AllowAll");  // Phải đặt trước MapControllers

// 8. Map controllers
app.MapControllers();

app.Run();