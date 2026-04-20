namespace TourGuideServer.DTOs
{
    public class POIDTO
    {
        public string POIID { get; set; } = string.Empty;
        public string RestaurantName { get; set; } = string.Empty;
        public string ShortDescription { get; set; } = string.Empty;
        public string NarrationText { get; set; } = string.Empty;
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string? Address { get; set; }
        public string? Category { get; set; }
        public string? Img { get; set; }
        public string? OwnerID { get; set; }
        public int ViewCount { get; set; }
        public int ListenCount { get; set; }
        public int Priority { get; set; }
    }
}