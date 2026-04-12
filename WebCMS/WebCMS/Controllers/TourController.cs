using Microsoft.AspNetCore.Mvc;
using WebCMS.Models; // Đảm bảo đúng namespace của bạn

namespace WebCMS.Controllers
{
    public class TourController : Controller
    {
        // Trang danh sách Tours
        public IActionResult Index()
        {
            // Tạm thời trả về danh sách trống hoặc dữ liệu mẫu
            var tours = new List<Tour>();
            return View(tours);
        }

        public IActionResult Create()
        {
            return View();
        }

        public IActionResult Edit(int id)
        {
            return View();
        }
    }
}