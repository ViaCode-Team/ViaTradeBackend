using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using ViaTrade.Configuration.Options;

namespace ViaTrade.Api.Attribute;

public class ServicePasswordAttribute() : ActionFilterAttribute, IAllowAnonymous
{
	public override void OnActionExecuting(ActionExecutingContext context)
	{
		var settings = context
			.HttpContext.RequestServices.GetRequiredService<IOptions<ServiceSecuritySettings>>()
			.Value;

		var expectedPassword = settings.Password;
		var providedPassword = context.HttpContext.Request.Headers["TgBot-Service-Password"].ToString();

		if (
			string.IsNullOrWhiteSpace(providedPassword)
			|| !string.Equals(providedPassword, expectedPassword, StringComparison.Ordinal)
		)
		{
			context.Result = new UnauthorizedResult();
		}
	}
}
