namespace WebCMS.Models
{
    public class POIDTO
    {
        public string? POIID { get; set; }
        public string? RestaurantName { get; set; } // Đổi từ Name thành RestaurantName
        public string? Category { get; set; }       // Đổi từ CategoryName thành Category
        public string? Address { get; set; }
        public string? Description { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string? Img { get; set; }
        public string OwnerID { get; set; } = string.Empty;
        public int ViewCount { get; set; }
        public int ListenCount { get; set; }
        public int Priority { get; set; }
    }
}