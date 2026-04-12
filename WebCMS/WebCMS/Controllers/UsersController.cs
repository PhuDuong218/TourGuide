using Microsoft.AspNetCore.Mvc;

namespace WebCMS.Controllers
{
    public class UsersController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}