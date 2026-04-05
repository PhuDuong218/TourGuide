namespace TourGuideServer.Models
{
    public class VisitHistory
    {
        public int Id { get; set; }

        public int UserID { get; set; }
        public int POIID { get; set; }

        public DateTime VisitTime { get; set; } = DateTime.Now;
        public POI? POI { get; set; }
        public User? User { get; set; }
    }
}