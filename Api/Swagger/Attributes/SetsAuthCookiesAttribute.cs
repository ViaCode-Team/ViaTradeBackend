namespace ViaTrade.Api.Swagger.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public sealed class SetsAuthCookiesAttribute : System.Attribute
{
	public int StatusCode { get; }

	public SetsAuthCookiesAttribute(int statusCode = StatusCodes.Status204NoContent)
	{
		StatusCode = statusCode;
	}
}
