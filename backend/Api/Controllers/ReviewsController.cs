using Core.DTOs;
using Core.Extensions;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/reviews")]
    [Authorize]
    public sealed class ReviewsController : ControllerBase
    {
        private readonly ReviewService _reviewService;

        public ReviewsController(ReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateReviewDto dto)
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized();

            await _reviewService.CreateOrUpdateAsync((int)userId, dto);
            return NoContent();
        }
    }
}
