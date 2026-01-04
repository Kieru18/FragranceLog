using Core.DTOs;
using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/home")]
    public sealed class HomeController : ControllerBase
    {
        private readonly IPerfumeAnalyticsService _service;

        public HomeController(IPerfumeAnalyticsService service)
        {
            _service = service;
        }

        [HttpGet("perfume-of-the-day")]
        public async Task<ActionResult<PerfumeOfTheDayDto>> GetPerfumeOfTheDay()
        {
            var result = await _service.GetPerfumeOfTheDayAsync();
            if (result == null)
                return NoContent();

            return Ok(result);
        }
    }
}
