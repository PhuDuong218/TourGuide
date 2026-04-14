using System.ComponentModel.DataAnnotations;

namespace TourGuideServer.Models
{
    public class QRCode
    {
        [Key]
        public string QRID { get; set; } = string.Empty;
        public string POIID { get; set; } = string.Empty; 
        public string QRValue { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public POI? POI { get; set; }
    }
}