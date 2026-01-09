using Core.DTOs;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/perfume-recognition")]
    public sealed class PerfumeRecognitionController : ControllerBase
    {
        private readonly IPerfumeRecognitionService _service;

        public PerfumeRecognitionController(IPerfumeRecognitionService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Recognize(
            [FromBody] PerfumeRecognitionRequestDto dto,
            CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(dto.ImageBase64))
                return BadRequest();

            var bytes = Convert.FromBase64String(dto.ImageBase64);
            using var ms = new MemoryStream(bytes);

            var results = await _service.RecognizeAsync(ms, dto.TopK, ct);
            return Ok(results);
        }
    }
}
