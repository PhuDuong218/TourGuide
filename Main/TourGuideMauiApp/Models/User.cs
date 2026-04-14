namespace TourGuideMauiApp.Models;

public class User
{
    public string UserID { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Role { get; set; }
}
