namespace WebCMS.Models
{
    public class DashboardViewModel
    {
        public int TotalLanguages { get; set; }

        public int TotalPOI { get; set; }
        public int TotalListens { get; set; }

        public int TotalQR { get; set; }
        public int ActiveToday { get; set; }

        public List<string> ChartLabels { get; set; } = new List<string>();
        public List<int> ChartValues { get; set; } = new List<int>();
    }
}