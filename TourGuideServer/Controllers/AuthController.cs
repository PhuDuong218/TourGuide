using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TourGuideServer.Data;

namespace TourGuideServer.Controllers
{
    // ── Request / Response DTOs ──────────────────────────────────────────────
    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponse
    {
        public int UserID { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty; // MVP: simple token
    }

    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        // POST /api/Auth/login
        // Body: { "username": "admin", "password": "123456" }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "Vui lòng nhập tên đăng nhập và mật khẩu." });
            }

            // NOTE: DB đang lưu plain text password (PasswordHash = '123456')
            // MVP: so sánh trực tiếp. Production: dùng BCrypt.Verify()
            var user = await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.Username == request.Username &&
                    u.PasswordHash == request.Password);

            if (user == null)
            {
                return Unauthorized(new { message = "Tên đăng nhập hoặc mật khẩu không đúng." });
            }

            // MVP: trả về thông tin user + token đơn giản
            // Production: dùng JWT (System.IdentityModel.Tokens.Jwt)
            var token = Convert.ToBase64String(
                System.Text.Encoding.UTF8.GetBytes(
                    $"{user.Username}:{user.UserID}:{DateTime.Now.Ticks}"));

            return Ok(new LoginResponse
            {
                UserID = user.UserID,
                Username = user.Username,
                FullName = user.FullName,
                Role = user.Role,
                Token = token
            });
        }
    }
}