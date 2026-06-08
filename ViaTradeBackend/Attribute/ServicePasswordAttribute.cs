using Domain.Models.ConfigOptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace ViaTradeBackend.Attribute
{
    public class ServicePasswordAttribute() : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var options = context.HttpContext.RequestServices
                .GetRequiredService<IOptions<ServiceSecurity>>();

            var expectedPassword = options.Value.Password;
            var providedPassword = context.HttpContext.Request.Headers["X-Service-Password"];

            if (string.IsNullOrWhiteSpace(providedPassword) ||
                !string.Equals(providedPassword, expectedPassword, StringComparison.Ordinal))
            {
                context.Result = new ForbidResult();
            }
        }
    }
}
