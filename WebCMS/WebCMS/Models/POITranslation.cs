namespace WebCMS.Models
{
    public class POITranslation
    {
        public int POIID { get; set; }
        public string LanguageCode { get; set; }
        public string DisplayName { get; set; }
        public string ShortDescription { get; set; }
        public string NarrationText { get; set; }
    }
}