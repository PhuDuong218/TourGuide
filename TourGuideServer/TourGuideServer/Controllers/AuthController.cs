namespace TourGuideServer.Controllers
{
    public class LoginResponse
    {
        public string UserID { get; set; } = string.Empty; 
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }
}