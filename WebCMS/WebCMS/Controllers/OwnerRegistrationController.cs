using Microsoft.AspNetCore.Mvc;
using WebCMS.Models;
using WebCMS.Services;

namespace WebCMS.Controllers
{
    public class OwnerRequestController : Controller
    {
        private readonly OwnerRequestService _service;

        public OwnerRequestController(OwnerRequestService service)
        {
            _service = service;
        }

        public async Task<IActionResult> Index()
        {
            // Lấy dữ liệu THẬT 100% từ Database
            var data = await _service.GetAllAsync();
            return View(data);
        }

        [HttpPost]
        public async Task<IActionResult> Approve(int id)
        {
            await _service.ApproveAsync(id);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Reject(int id)
        {
            await _service.RejectAsync(id);
            return RedirectToAction("Index");
        }
    }
}