namespace WebCMS.Services
{
    using WebCMS.Models;

    public interface IPOIService

    {
            Task<List<POI>> GetAllAsync();
            Task<POI?> GetByIdAsync(string id);
            Task<bool> CreateAsync(POI poi);
            Task<bool> UpdateAsync(POI poi);
            Task<bool> DeleteAsync(string id);
    }
}
