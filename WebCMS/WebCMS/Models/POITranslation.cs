
namespace WebCMS.Models
{
    public class POITranslation
    {
        public int TranslationID { get; set; }
        public string POIID { get; set; } = string.Empty;

        public string LanguageCode { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string ShortDescription { get; set; } = string.Empty;
        public string NarrationText { get; set; } = string.Empty;
    }
}