namespace TourGuideServer.DTOs
{
    public class POIDTO
    {
        public int POIID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Narration { get; set; } = string.Empty;
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }

        // Thêm từ bảng POI
        public string? Address { get; set; }
        public string? Category { get; set; }
        public string? ImageUrl { get; set; }
    }
}