using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
//using Core.Entities;
//using Infrastructure.Data;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PerfumeController : ControllerBase
    {
        //private readonly FragranceLogContext _context;

        public PerfumeController()
        //public PerfumeController(FragranceLogContext context)
        {
            //_context = context;
        }

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
