namespace TourGuideServer.Models
{
    public class POITranslation
    {
        public int TranslationID { get; set; }
        public int POIID { get; set; }
        public string LanguageCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;
        public string ShortDescription { get; set; } = string.Empty;
        public string NarrationText { get; set; } = string.Empty;

    }
}
