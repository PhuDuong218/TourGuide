using Microsoft.AspNetCore.Mvc;

namespace WebCMS.Controllers
{
    public class QRCodeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}