namespace ViaTrade.Api.Swagger.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public sealed class DeletesAuthCookiesAttribute(int statusCode = StatusCodes.Status204NoContent) : Attribute
{
	public int StatusCode { get; } = statusCode;
}
