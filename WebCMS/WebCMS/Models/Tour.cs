namespace WebCMS.Models
{
    public class Tour
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Duration { get; set; }
        public List<string> PoiIds { get; set; } = new();
    }
}
