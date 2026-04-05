using Microsoft.AspNetCore.Mvc;
using TourGuideServer.Services;

namespace TourGuideServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class POIController : ControllerBase
    {
        private readonly POIService _service;

        public POIController(POIService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> Get(string lang = "vi")
        {
            var data = await _service.GetPOIsAsync(lang);
            return Ok(data);
        }
    }
}