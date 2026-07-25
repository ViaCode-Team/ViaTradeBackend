namespace Application.Common.Exceptions;

public class BadRequestException(string message, string code = "bad_request", Exception? innerException = null)
	: AppException(message, code, innerException) { }
