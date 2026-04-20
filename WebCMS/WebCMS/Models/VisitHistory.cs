namespace WebCMS.Models
{
    public class VisitHistory
    {
        public string VisitID { get; set; } = default!;
        public string UserID { get; set; } = default!;
        public string POIID { get; set; } = default!;
        public DateTime VisitTime { get; set; }
        public string ScanMethod { get; set; } = default!;

        public POI POI { get; set; } = default!;
    }
}