namespace Application.Common.Exceptions;

public class InvalidTokenException(string message = "The token is invalid or expired.")
	: AuthenticationException(message, "invalid_token") { }
