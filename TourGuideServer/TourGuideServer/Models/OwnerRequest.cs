using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TourGuideServer.Models
{
    [Table("OwnerRequests")]
    public class OwnerRequest
    {
        [Key]
        public int RequestID { get; set; }
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? PlaceName { get; set; }
        public string? Address { get; set; }
        public string? Status { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}