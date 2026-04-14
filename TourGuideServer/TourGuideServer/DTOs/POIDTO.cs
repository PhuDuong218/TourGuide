namespace TourGuideServer.DTOs
{
    public class POIDTO
    {
        public string POIID { get; set; } = string.Empty; 
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Narration { get; set; } = string.Empty;
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string? Address { get; set; }
        public string? Category { get; set; }
        public string? ImageUrl { get; set; } 
    }
}