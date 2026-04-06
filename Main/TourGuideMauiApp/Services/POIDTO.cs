namespace TourGuideMauiApp.Models
{
    public class POIDTO
    {
        public int POIID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Narration { get; set; } = string.Empty;

        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public string? Address { get; set; }
        public string? ImageUrl { get; set; }
        public string DistanceText { get; set; } = string.Empty;
    }
}