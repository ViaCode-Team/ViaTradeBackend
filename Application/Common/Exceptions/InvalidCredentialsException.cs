namespace Application.Common.Exceptions;

public class InvalidCredentialsException(string message = "Invalid login or password.")
	: AuthenticationException(message, "invalid_credentials") { }
