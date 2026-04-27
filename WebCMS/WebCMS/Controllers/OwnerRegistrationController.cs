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
        private readonly OwnerRequestService _service;

        public OwnerRegistrationController(OwnerRequestService service)
        {
            _service = service;
        }

        public async Task<IActionResult> Index()
        {
            var requests = await _service.GetAllAsync();

            // Đưa các yêu cầu "Pending" (Chờ duyệt) lên đầu bảng
            var sortedRequests = requests
                .OrderByDescending(r => r.Status == "Pending")
                .ToList();

            return View(sortedRequests);
        }

        [HttpPost]
        public async Task<IActionResult> Approve(string id)
        {
            bool success = await _service.ApproveAsync(id);
            if (success)
            {
                TempData["SuccessMessage"] = "Đã duyệt và nâng cấp quyền Chủ quán thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi duyệt yêu cầu.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Reject(string id)
        {
            bool success = await _service.RejectAsync(id);
            if (success)
            {
                TempData["SuccessMessage"] = "Đã từ chối yêu cầu.";
            }

            return RedirectToAction("Index");
        }
    }
}