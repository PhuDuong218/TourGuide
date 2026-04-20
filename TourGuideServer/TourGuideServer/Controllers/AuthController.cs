using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TourGuideServer.Data;
using TourGuideServer.Models;

namespace TourGuideServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        // Lớp nhận dữ liệu từ App MAUI gửi lên
        public class LoginRequest
        {
            public string Username { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // Kiểm tra khớp Username và Password trong Database
            var user = await _context.Users.FirstOrDefaultAsync(u =>
                u.Username == request.Username &&
                u.PasswordHash == request.Password);

            if (user == null)
            {
                // Trả về lỗi 401 nếu sai tài khoản/mật khẩu
                return Unauthorized(new { message = "Sai tên đăng nhập hoặc mật khẩu" });
            }

            // Nếu đúng, trả về UserID (Ví dụ: U003)
            return Ok(new { userId = user.UserID, fullName = user.FullName, role = user.Role });
        }

        // Lớp nhận dữ liệu đăng ký
        public class RegisterRequest
        {
            public string FullName { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;    // Cột Email riêng
            public string Username { get; set; } = string.Empty; // Tên đăng nhập riêng
            public string Password { get; set; } = string.Empty;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            // Kiểm tra xem Username đã tồn tại chưa
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
                return BadRequest(new { message = "Tên đăng nhập đã tồn tại!" });

            // Tạo ID mới
            var count = await _context.Users.CountAsync();
            string newId = "U" + (count + 1).ToString("D3");

            var newUser = new User
            {
                UserID = newId,
                FullName = request.FullName,
                Email = request.Email,       
                Username = request.Username,
                PasswordHash = request.Password,
                Role = "user",
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đăng ký thành công" });
        }
    }
}