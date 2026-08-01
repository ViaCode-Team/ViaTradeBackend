namespace Application.Common.Exceptions;

public class ValidationException(
	string message,
	IReadOnlyDictionary<string, string[]> errors,
	string code = "validation_failed"
) : AppException(message, code)
{
	public IReadOnlyDictionary<string, string[]> Errors { get; } = errors;
}
