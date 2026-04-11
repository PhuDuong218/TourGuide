namespace TourGuideServer.Models
{
    public class POI
    {
        public int POIID { get; set; }
        public string RestaurantName { get; set; } = string.Empty;
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string Address { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }

        public int? OwnerID { get; set; }

        public ICollection<POITranslation> Translations { get; set; } = new List<POITranslation>();
    }
}
