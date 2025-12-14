using Core.DTOs;
using Core.Extensions;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewsController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        [HttpPost]
        public async Task<IActionResult> SaveReview(CreateReviewDto dto)
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized();

            await _reviewService.CreateOrUpdateAsync((int)userId, dto);
            return NoContent();
        }
    }
}
