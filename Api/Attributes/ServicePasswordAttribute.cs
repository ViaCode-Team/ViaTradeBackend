using System.Runtime.InteropServices;
using System.Security.Cryptography;
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
		var providedPassword = context.HttpContext.Request.Headers["Service-Password"].ToString();

		if (
			string.IsNullOrWhiteSpace(providedPassword)
			|| !IsPasswordValidConstantTime(providedPassword, expectedPassword)
		)
			context.Result = new UnauthorizedResult();
	}

	private static bool IsPasswordValidConstantTime(string providedPassword, string expectedPassword)
	{
		var expectedBytes = MemoryMarshal.AsBytes(expectedPassword.AsSpan());
		var providedBytes = MemoryMarshal.AsBytes(providedPassword.AsSpan());

		bool isLengthEqual = expectedBytes.Length == providedBytes.Length;
		bool isContentEqual = CryptographicOperations.FixedTimeEquals(
			expectedBytes,
			isLengthEqual ? providedBytes : expectedBytes
		);

		return isLengthEqual && isContentEqual;
	}
}
