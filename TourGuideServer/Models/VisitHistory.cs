namespace TourGuideServer.Models
{
    public class VisitHistory
    {
        public int VisitID { get; set; }        
        public int? UserID { get; set; }        
        public int POIID { get; set; }
        public DateTime VisitTime { get; set; } = DateTime.Now;
        public string? ScanMethod { get; set; }          // 'GPS_Trigger' | 'QR_Scan'
        public decimal? UserLat { get; set; }          // Tọa độ thực tế của khách
        public decimal? UserLon { get; set; }
        public string? LanguageUsed { get; set; }          // 'vi', 'en', ...

        // Navigation
        public POI? POI { get; set; }
        public User? User { get; set; }
    }
}