using System.ComponentModel.DataAnnotations;

namespace TourGuideServer.Models
{
    public class QRCode
    {
        [Key]
        public int QRID { get; set; }
        public int POIID { get; set; }
        public string QRValue { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation
        public POI? POI { get; set; }
    }
}