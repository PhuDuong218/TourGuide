using System.ComponentModel.DataAnnotations;

namespace TourGuideServer.Models
{
    public class User
    {
        [Key]
        public string UserID { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? PasswordHash { get; set; } // Dùng cột này để so khớp mật khẩu
        public string? Email { get; set; }
        public string? Role { get; set; }
        public DateTime? CreatedAt { get; set; } = DateTime.Now;
    }
}