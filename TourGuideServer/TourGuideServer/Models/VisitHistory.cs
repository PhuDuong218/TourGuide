namespace TourGuideServer.Models
{
    public class VisitHistory
    {
        public string VisitID { get; set; } = string.Empty; 
        public string? UserID { get; set; }                
        public string POIID { get; set; } = string.Empty; 
        public DateTime VisitTime { get; set; } = DateTime.Now;
        public string? ScanMethod { get; set; }
        public decimal? UserLat { get; set; }
        public decimal? UserLon { get; set; }
        public string? LanguageUsed { get; set; }

        public POI? POI { get; set; }
        public User? User { get; set; }
    }
}