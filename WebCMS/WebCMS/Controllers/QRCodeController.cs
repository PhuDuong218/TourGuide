using Microsoft.AspNetCore.Mvc;

public class QRCodeController : Controller
{
    private readonly IConfiguration _configuration;
    public QRCodeController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public IActionResult Index()
    {
        // Lấy URL API (ví dụ http://localhost:5015) truyền xuống View
        ViewBag.ApiUrl = _configuration["ApiSettings:BaseUrl"];
        return View();
    }
}