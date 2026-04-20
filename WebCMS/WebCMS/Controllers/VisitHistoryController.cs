using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebCMS.Services;

namespace WebCMS.Controllers
{
    // 🔥 Bắt buộc đăng nhập mới được vào
    [Authorize(Roles = "admin,owner")]
    public class VisitHistoryController : Controller
    {
        private readonly VisitHistoryService _service;

        public VisitHistoryController(VisitHistoryService service)
        {
            _service = service;
        }

        public async Task<IActionResult> Index()
        {
            // Lấy toàn bộ lịch sử từ API
            var data = await _service.GetAllAsync();

            // 🔥 PHÂN QUYỀN: Nếu là Owner, chỉ hiển thị lịch sử của POI do họ quản lý
            if (User.IsInRole("owner"))
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                // Lưu ý: Đảm bảo class VisitHistory của WebCMS đã có thuộc tính OwnerID
                data = data.Where(x => x.OwnerID == userId).ToList();
            }

            // Sắp xếp lượt tham quan mới nhất lên đầu bảng
            var sortedData = data.OrderByDescending(x => x.VisitTime).ToList();

            return View(sortedData);
        }
    }
}