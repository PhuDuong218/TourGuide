namespace WebCMS.Models
{
    public class VisitHistory
    {
        public string VisitID { get; set; } = string.Empty;
        public string? UserID { get; set; }
        public string POIID { get; set; } = string.Empty;
        public DateTime VisitTime { get; set; }
        public string? ScanMethod { get; set; }
        public string? OwnerID { get; set; }

        public string? POIName { get; set; }
        public string? Username { get; set; }

        public decimal? UserLat { get; set; }
        public decimal? UserLon { get; set; }
        public string? LanguageUsed { get; set; }
    }
}