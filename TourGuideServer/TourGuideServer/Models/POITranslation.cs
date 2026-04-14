namespace TourGuideServer.Models
{
    public class POITranslation
    {
        public string TranslationID { get; set; } = string.Empty; 
        public string POIID { get; set; } = string.Empty;        
        public string LanguageCode { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string ShortDescription { get; set; } = string.Empty;
        public string NarrationText { get; set; } = string.Empty;
    }
}