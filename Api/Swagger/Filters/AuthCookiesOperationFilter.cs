using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using ViaTrade.Api.Swagger.Attributes;
using ViaTrade.Configuration.Options;

namespace ViaTrade.Api.Swagger.Filters;

public sealed class AuthCookiesOperationFilter(IOptions<AuthCookieSettings> authCookieOptions) : IOperationFilter
{
	private readonly AuthCookieSettings _authCookieOptions = authCookieOptions.Value;

	public void Apply(OpenApiOperation operation, OperationFilterContext context)
	{
		var authCookiesMetadata = context
			.ApiDescription.ActionDescriptor.EndpointMetadata.OfType<SetsAuthCookiesAttribute>()
			.SingleOrDefault();

		if (authCookiesMetadata == null)
			return;

		var statusCode = authCookiesMetadata.StatusCode.ToString();
		var response = GetOrCreateResponse(operation, statusCode);

		response.Description = "Authentication succeeded. Access and refresh tokens are set as secure cookies.";
		response.Headers ??= new Dictionary<string, IOpenApiHeader>();
		response.Headers["Set-Cookie"] = new OpenApiHeader
		{
			Description =
				$"Sent twice: `{_authCookieOptions.AccessTokenCookie}` and "
				+ $"`{_authCookieOptions.RefreshTokenCookie}`. Both cookies are HttpOnly, Secure, "
				+ "SameSite=Strict, and Path=/.",
			Schema = new OpenApiSchema { Type = JsonSchemaType.String },
		};
	}

	private static OpenApiResponse GetOrCreateResponse(OpenApiOperation operation, string statusCode)
	{
		var responses = operation.Responses ??= [];
		var hasResponse = responses.TryGetValue(statusCode, out var response);

		if (hasResponse && response is OpenApiResponse openApiResponse)
			return openApiResponse;

		var createdResponse = new OpenApiResponse();
		responses[statusCode] = createdResponse;

		return createdResponse;
	}
}
