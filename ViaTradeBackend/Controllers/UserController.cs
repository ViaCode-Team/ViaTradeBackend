using Application.Interfaces.Auth;
using Infrastructure.Repositoryes.DataBase;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ViaTradeBackend.Models.User;

namespace ViaTradeBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController(UserRepository userRepository, IJwtHelper jwtHelper,
        UserService userService) : ControllerBase
    {
        private readonly UserRepository _userRepository = userRepository;
        private readonly IJwtHelper _jwtHelper = jwtHelper;
        private readonly UserService _userService = userService;

        [HttpGet("me")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<MeDto>> GetMe(CancellationToken cancellationToken)
        {
            var userId = _jwtHelper.GetUserIdFromClaims(User);
            var user = await _userService.EnsureUserAsunc(userId, cancellationToken);

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
