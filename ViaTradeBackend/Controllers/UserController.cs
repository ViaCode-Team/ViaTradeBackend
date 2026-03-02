using Application.Intarfaces;
using Application.Intarfaces.Auth;
using Application.Intarfaces.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ViaTradeBackend.Models.User;

namespace ViaTradeBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController(IUserRepository userRepository, IJwtHelper jwtHelper, 
        IUserService userService) : ControllerBase
    {
        private readonly IUserRepository _userRepository = userRepository;
        private readonly IJwtHelper _jwtHelper = jwtHelper;
        private readonly IUserService _userService = userService;

        [HttpGet("me")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<MeDto>> GetMe(CancellationToken cancellationToken)
        {
            var userId = _jwtHelper.GetUserIdFromClaims(User);

            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null) return NotFound();

            return Ok(new MeDto
            {
                Id = user.Id,
                Login = user.Login,
                LastLoginDate = user.LastLoginDate,
                TgId = user.TgId
            });
        }
    }
}
