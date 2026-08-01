namespace Application.Common.Exceptions;

public abstract class AppException(string message, string code, Exception? innerException = null)
	: Exception(message, innerException)
{
	public string Code { get; } = code;
}
