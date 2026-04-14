using WebCMS.Models;

namespace WebCMS.Services
{
    public interface IPOIService
    {
        Task<List<POI>> GetAllAsync();

        Task<POI?> GetByIdAsync(string id);

        Task CreateAsync(POI poi, IFormFile? imageFile);

        Task UpdateAsync(string id, POI poi, IFormFile? imageFile);

        Task DeleteAsync(string id);
    }
}