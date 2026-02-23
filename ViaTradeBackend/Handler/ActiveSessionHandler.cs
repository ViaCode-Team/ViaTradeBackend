using System.IdentityModel.Tokens.Jwt;
using Application.Intarfaces;
using Microsoft.AspNetCore.Authorization;

namespace ViaTradeBackend.Handler
{
    public class ActiveSessionRequirement : IAuthorizationRequirement { }

    public class ActiveSessionHandler : AuthorizationHandler<ActiveSessionRequirement>
    {
        private readonly ISessionRepository _sessionRepository;

        public ActiveSessionHandler(ISessionRepository sessionRepository)
        {
            _sessionRepository = sessionRepository;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            ActiveSessionRequirement requirement)
        {
            var sessionId = context.User.Claims
                .FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;

            if (string.IsNullOrEmpty(sessionId))
            {
                context.Fail();
                return;
            }

            var session = await _sessionRepository.GetAsync(sessionId);

            if (session != null)
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail();
            }
        }
    }
}
