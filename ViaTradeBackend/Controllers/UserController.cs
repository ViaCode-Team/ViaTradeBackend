using System.ComponentModel.DataAnnotations;
using Application.Interfaces.Auth;
using Domain.Entities.DataBase;
using Domain.Models.Dto.User;
using Domain.Models.Request.Auth;
using Infrastructure.Repositories.DataBase;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ViaTradeBackend.Attribute;

namespace ViaTradeBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController(
        UserRepository userRepository,
        IJwtHelper jwtHelper,
        UserService userService,
        ILogger<UserController> logger) : ControllerBase
    {
        private readonly UserRepository _userRepository = userRepository;
        private readonly IJwtHelper _jwtHelper = jwtHelper;
        private readonly UserService _userService = userService;
        private readonly ILogger<UserController> _logger = logger;

        [Authorize]
        [HttpGet("me")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<MeDto>> GetMe(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting current user information");
            var userId = _jwtHelper.GetUserIdFromClaims(User);
            var user = await _userService.EnsureUserAsync(userId, cancellationToken);

            return Ok(new MeDto
            {
                Id = user.Id,
                Login = user.Login,
                LastLoginDate = user.LastLoginDate,
                RegisterDate = user.RegisterDate,
                TgId = user.TgId
            });
        }

        [Authorize]
        [HttpGet("tgToken")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<TgTokenResponse>> GetTgToken(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Generating Telegram token for user");
            var userId = _jwtHelper.GetUserIdFromClaims(User);
            var user = await _userService.EnsureUserAsync(userId, cancellationToken);

            var response = new TgTokenResponse
            {
                TgToken = await _userService.GenerateTgLink(user.Id)
            };

            return Ok(response);
        }

        [ServicePassword]
        [HttpPost("tgToken")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        public async Task<ActionResult> PostTgToken(
            [FromBody, Required] TgTokenRequest tgTokenRequest,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Processing Telegram token for user");
            var userId = await _userService.GetUserId(tgTokenRequest.TgToken)
                ?? throw new NullReferenceException(nameof(tgTokenRequest.TgToken));

            var user = await _userRepository.GetByIdAsync(userId, cancellationToken)
                ?? throw new NullReferenceException(nameof(tgTokenRequest.TgToken)); ;

            user.TgId = tgTokenRequest.TgId;
            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync();

            _logger.LogInformation("Telegram token processed successfully for user: {UserId}", userId);
            return Accepted();
        }

        [ServicePassword]
        [HttpGet("user")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<User>>> GetUser(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting all users with Telegram links");
            return Ok(await _userRepository.GetAllWithTgLikn(cancellationToken));
        }
    }
}
