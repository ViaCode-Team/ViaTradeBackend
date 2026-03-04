using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ViaTradeBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NoteController() : ControllerBase
    {
        [HttpGet("test")]
        public async Task<IActionResult> test()
        {
            return Ok();
        }
    }
}
