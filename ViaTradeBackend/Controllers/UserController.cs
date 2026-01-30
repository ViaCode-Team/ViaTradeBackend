using Application.Intarfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ViaTradeBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController(IUserRepository userRepository, IJwtHelper jwtHelper) : ControllerBase
    {
        private readonly IUserRepository _userRepository = userRepository;
        private readonly IJwtHelper _jwtHelper = jwtHelper;

        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            var userId = _jwtHelper.GetUserIdFromClaims(User);

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return NotFound();

            return Ok(new
            {
                user.Login,
                user.LastLoginDate,
                user.TgId
            });
        }
    }
}
