using WebCMS.Models;

namespace WebCMS.Services
{
    public interface IVisitHistoryService
    {
        Task<List<VisitHistory>> GetAllAsync();
    }
}