using Core.DTOs;
using Core.Extensions;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        public async Task<IActionResult> SaveReview(SaveReviewDto dto)
        {
            var userId = User.GetUserId();
            if (userId == null)
                return Unauthorized();

            await _reviewService.CreateOrUpdateAsync((int)userId, dto);
            return NoContent();
        }


        [HttpGet("current")]
        public async Task<ActionResult<ReviewDto>> GetCurrentUserReview([FromQuery] int perfumeId)
        {
            var userId = User.GetUserId();

            if (userId == null)
                return Unauthorized();

            var review = await _reviewService.GetByUserAndPerfumeAsync(userId ?? 0, perfumeId);

            if (review == null)
                return NotFound();

            return Ok(review);
        }
    }
}
