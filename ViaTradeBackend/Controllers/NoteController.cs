using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ViaTradeBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NoteController() : ControllerBase
    {
        [HttpGet("byuser/instrument")]
        public async Task<IActionResult> GetByUserInstrumentAll()
        {
            return Ok();
        }

        [HttpGet("byuser/instrument/{id}")]
        public async Task<IActionResult> test()
        {
            return Ok();
        }
    }
}
