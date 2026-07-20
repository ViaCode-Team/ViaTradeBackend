namespace Application.Common.Exceptions;

public class NotFoundException(
	string message = "The requested resource was not found.",
	string code = "not_found",
	Exception? innerException = null
) : AppException(message, code, innerException) { }
