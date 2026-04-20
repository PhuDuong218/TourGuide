using System;
using System.ComponentModel.DataAnnotations; // 🔥 BẮT BUỘC phải có dòng này

namespace TourGuideServer.Models
{
    public class OwnerRequest
    {
        [Key] // 🔥 Thêm dòng này để báo cho EF biết đây là Khóa chính
        public string RequestID { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string PlaceName { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? Status { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}