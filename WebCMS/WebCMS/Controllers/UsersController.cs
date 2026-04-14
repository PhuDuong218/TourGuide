using Microsoft.AspNetCore.Mvc;

namespace WebCMS.Controllers
{
    public class UsersController : Controller
    {
        private readonly IConfiguration _configuration;
        public UsersController(IConfiguration configuration) => _configuration = configuration;

        public IActionResult Index()
        {
            ViewBag.ApiUrl = _configuration["ApiSettings:BaseUrl"];
            return View();
        }
    }
}