namespace ViaTrade.Application.Common.Exceptions;

public class AuthenticationException(string message = "Authentication is required.", string code = "unauthorized")
	: AppException(message, code) { }
