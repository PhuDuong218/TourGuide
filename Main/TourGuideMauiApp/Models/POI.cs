namespace TourGuideMauiApp.Models;

public class POI
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? Narration { get; set; }
    public string? Img { get; set; }
}