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
		var metadata = context.ApiDescription.ActionDescriptor.EndpointMetadata;
		var setsAuthCookies = metadata.OfType<SetsAuthCookiesAttribute>().SingleOrDefault();
		var deletesAuthCookies = metadata.OfType<DeletesAuthCookiesAttribute>().SingleOrDefault();
		var statusCode = deletesAuthCookies?.StatusCode ?? setsAuthCookies?.StatusCode;

		if (statusCode is null)
			return;

		var response = GetOrCreateResponse(operation, statusCode.Value.ToString());
		var headerDescription = deletesAuthCookies switch
		{
			null => "Sets cookies containing the access and refresh tokens.",
			_ => "Clears the access and refresh token cookies by sending empty values and an expiration date in the past.",
		};
		headerDescription +=
			$" The response includes one Set-Cookie header for `{_authCookieOptions.AccessTokenCookie}` "
			+ $"and one for `{_authCookieOptions.RefreshTokenCookie}`."
			+ " Both cookies use HttpOnly, Secure, SameSite=Strict, and Path=/.";

		response.Headers ??= new Dictionary<string, IOpenApiHeader>();
		response.Headers["Set-Cookie"] = new OpenApiHeader
		{
			Description = headerDescription,
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
