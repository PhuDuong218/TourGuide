
    namespace WebCMS.Models
    {
        public class VisitStatsDTO
        {
            public string POIID { get; set; } = string.Empty;
            public string POIName { get; set; } = string.Empty;

            public int Total { get; set; }
            public int ByGPS { get; set; }
            public int ByQR { get; set; }

            public DateTime LastVisit { get; set; }
        }
    }
