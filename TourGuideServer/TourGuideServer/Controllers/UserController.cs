using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TourGuideServer.Data;
using TourGuideServer.Models;

namespace TourGuideServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;
        public UserController(AppDbContext context) => _context = context;

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _context.Users
                .OrderBy(u => u.UserID)
                .Select(u => new { u.UserID, u.Username, u.Email, u.FullName, u.Role })
                .ToListAsync();
            return Ok(users);
        }

        [HttpGet("{id}")] // Xóa :int nếu ID của bạn là chuỗi (như U001)
        public async Task<IActionResult> GetById(string id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();
            return Ok(new { user.UserID, user.Username, user.Email, user.FullName, user.Role });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUserRequest req)
        {
            var exists = await _context.Users.AnyAsync(u => u.Username == req.Username);
            if (exists) return Conflict(new { message = $"Username '{req.Username}' đã tồn tại." });

            var user = new User
            {
                Username = req.Username,
                Email = req.Email,
                PasswordHash = req.Password,
                FullName = req.FullName,
                Role = req.Role
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return Ok(new { user.UserID, user.Username, user.Email, user.FullName, user.Role });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateUserRequest req)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            user.Email = req.Email;
            user.FullName = req.FullName;
            user.Role = req.Role;
            if (!string.IsNullOrEmpty(req.Password)) user.PasswordHash = req.Password;

            await _context.SaveChangesAsync();
            return Ok(new { user.UserID, user.Username, user.Email, user.FullName, user.Role });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }

    // Các lớp DTO đi kèm
    public class CreateUserRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = "user";
    }

    public class UpdateUserRequest
    {
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = "user";
        public string? Password { get; set; }
    }
}