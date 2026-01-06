using Core.DTOs;
using Core.Extensions;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/home")]
    public sealed class HomeController : ControllerBase
    {
        private readonly IPerfumeAnalyticsService _perfumeAnalyticsService;
        private readonly IHomeInsightService _homeInsightService;

        public HomeController(IPerfumeAnalyticsService perfumeAnalyticsService, IHomeInsightService homeInsightService)
        {
            _perfumeAnalyticsService = perfumeAnalyticsService;
            _homeInsightService = homeInsightService;
        }

        [HttpGet("perfume-of-the-day")]
        public async Task<ActionResult<PerfumeOfTheDayDto>> GetPerfumeOfTheDay()
        {
            var result = await _perfumeAnalyticsService.GetPerfumeOfTheDayAsync();
            if (result == null)
                return NoContent();

            return Ok(result);
        }

        [HttpGet("recent-reviews")]
        public async Task<ActionResult<IReadOnlyList<HomeRecentReviewDto>>> GetRecentReviews(
            [FromQuery] int take = 3,
            CancellationToken ct = default)
        {
            return Ok(await _perfumeAnalyticsService.GetRecentReviewsAsync(take, ct));
        }

        [HttpGet("stats")]
        public async Task<ActionResult<HomeStatsDto>> GetStats()
        {
            return Ok(await _perfumeAnalyticsService.GetStatsAsync(default));
        }

        [HttpGet("insights")]
        public async Task<ActionResult<IReadOnlyList<HomeInsightDto>>> GetInsights(
            CancellationToken ct)
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized();

            var insights = await _homeInsightService.GetInsightsAsync(userId.Value, ct);

            return insights.Count == 0
                ? NoContent()
                : Ok(insights);
        }

        [HttpGet("top-from-country")]
        public async Task<IActionResult> GetTopFromCountry(
            [FromQuery] double lat,
            [FromQuery] double lng,
            CancellationToken ct)
        {
            var result = await _perfumeAnalyticsService.GetTopFromCountryAsync(lat, lng, 3, ct);
            return Ok(result);
        }
    }
}
