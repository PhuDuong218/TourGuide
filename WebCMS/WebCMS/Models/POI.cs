namespace WebCMS.Models
{
    public class POI
    {
        // SỬA: Đổi Id thành POIID
        public string POIID { get; set; } = string.Empty;

        public string RestaurantName { get; set; } = string.Empty;
        public string? Address { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int Radius { get; set; }
        public string? Type { get; set; }


        // SỬA: Đổi Image thành Img
        public string? Img { get; set; }

        public string? Category { get; set; }
        public string? Narration { get; set; }
        public virtual ICollection<POITranslation> Translations { get; set; } = new List<POITranslation>();
        public int ViewCount { get; set; } = 0;
        public int ListenCount { get; set; } = 0;
        public int Priority { get; set; } = 1;
    }
}