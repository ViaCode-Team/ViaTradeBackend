namespace ViaTrade.Application.Common.Exceptions;

public class ForbiddenException(
	string message = "Access to the requested resource is forbidden.",
	string code = "forbidden"
) : AppException(message, code) { }
