using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebCMS.Models
{
    [Table("OwnerRequests")] // Trỏ đúng tên bảng trong SQL
    public class OwnerRequest
    {
        [Key]
        public int RequestID { get; set; }
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? PlaceName { get; set; }
        public string? Address { get; set; }
        public string? Status { get; set; } // Pending, Approved, Rejected
        public DateTime? CreatedAt { get; set; }
    }
}