namespace TourGuideMauiApp.Models;

public class POIDTO
{
    public string POIID { get; set; } = string.Empty;
    public string RestaurantName { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public string NarrationText { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Img { get; set; } 
    public double Latitude { get; set; }
    public double Longitude { get; set; }

    public string? CategoryName { get; set; }
    public string DistanceText { get; set; } = string.Empty;
}
