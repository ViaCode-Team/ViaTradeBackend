using System.Security.Claims;
using Application.Intarfaces;
using Domain.Models;
using Infrastructure.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ViaTradeBackend.Models.Auth;

namespace ViaTradeBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthService authService, IJwtHelper jwtHelper) : ControllerBase
    {
        private readonly IAuthService _authService = authService;
        private readonly IJwtHelper _jwtHelper = jwtHelper;

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var clientId = GetClientId();
            var userAgent = Request.Headers.UserAgent.ToString();

            var result = await _authService.LoginAsync(
                request.Login,
                request.Password,
                clientId,
                userAgent);

            SetAuthCookies(result);
            return NoContent();
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var result = await _authService.RegisterAsync(request.Login, request.Password);

            SetAuthCookies(result);

            return NoContent();
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            if (!Request.Cookies.TryGetValue("refresh_token", out var refreshToken))
                throw new UnauthorizedAccessException();

            var clientId = GetClientId();

            var result = await _authService.RefreshTokenAsync(refreshToken, clientId);

            SetAuthCookies(result);
            return NoContent();
        }


        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            if (Request.Cookies.TryGetValue("refresh_token", out var refreshToken))
                await _authService.LogoutSessionAsync(refreshToken);

            Response.Cookies.Delete("access_token");
            Response.Cookies.Delete("refresh_token");

            return NoContent();
        }

        [HttpPost("logout-all")]
        [Authorize]
        public async Task<IActionResult> LogoutAll()
        {
            var userId = _jwtHelper.GetUserIdFromClaims(User);

            await _authService.LogoutAllAsync(userId);

            Response.Cookies.Delete("access_token");
            Response.Cookies.Delete("refresh_token");

            return NoContent();
        }

        [HttpGet("sessions")]
        [Authorize]
        public async Task<IActionResult> GetSessions()
        {
            var userId = _jwtHelper.GetUserIdFromClaims(User);

            var sessions = await _authService.GetUserSessionsAsync(userId);

            // Формируем DTO для фронта (можно убрать лишние данные, например session token)
            var result = sessions.Select(s => new
            {
                s.Id,
                s.ClientId,
                s.UserAgent,
                s.CreatedAt,
                s.LastSeen
            });

            return Ok(result);
        }


        private string GetClientId()
        {
            if (Request.Cookies.TryGetValue("client_id", out var clientId))
                return clientId;

            clientId = Guid.NewGuid().ToString();

            Response.Cookies.Append(
                "client_id",
                clientId,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddYears(1),
                    Path = "/"
                });

            return clientId;
        }

        private void SetAuthCookies(AuthResult result)
        {
            Response.Cookies.Append(
                "access_token",
                result.AccessToken,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddHours(1),
                    Path = "/"
                });

            Response.Cookies.Append(
                "refresh_token",
                result.RefreshToken,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddDays(7),
                    Path = "/"
                });
        }
    }
}
