//using Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PerfumesController : ControllerBase
    {
        //private readonly FragranceLogContext _context;

        //public PerfumesController(FragranceLogContext context)
        //{
        //    _context = context;
        //}

        [HttpGet]
        public async Task<IActionResult> GetPerfumes()
        {
            //var perfumes = await _context.Perfumes
            //    .Take(20)
            //    .ToListAsync();

            //return Ok(perfumes);
            return Ok();
        }
    }
}
