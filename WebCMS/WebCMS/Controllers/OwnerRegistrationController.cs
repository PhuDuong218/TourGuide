using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebCMS.Models;
using WebCMS.Services;

namespace WebCMS.Controllers
{
    // 🔥 Chỉ có Admin mới có quyền duyệt
    [Authorize(Roles = "admin")]
    public class OwnerRegistrationController : Controller
    {
        private readonly OwnerRequestService _service; // Gọi đúng tên Service của bạn

        public OwnerRegistrationController(OwnerRequestService service)
        {
            _service = service;
        }

        // Hiển thị danh sách
        public async Task<IActionResult> Index()
        {
            var requests = await _service.GetAllAsync();

            // Đưa các yêu cầu "Pending" (Chờ duyệt) lên đầu bảng
            var sortedRequests = requests
                .OrderByDescending(r => r.Status == "Pending")
                .ToList();

            return View(sortedRequests);
        }

        // Xử lý nút Duyệt
        [HttpPost]
        public async Task<IActionResult> Approve(string id) // Đổi int -> string
        {
            await _service.ApproveAsync(id);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Reject(string id) // Đổi int -> string
        {
            await _service.RejectAsync(id);
            return RedirectToAction("Index");
        }
    }
}