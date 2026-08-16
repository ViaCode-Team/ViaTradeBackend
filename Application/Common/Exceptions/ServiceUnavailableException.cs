namespace ViaTrade.Application.Common.Exceptions;

public class ServiceUnavailableException(
	string message,
	string code = "service_unavailable",
	Exception? innerException = null
) : AppException(message, code, innerException) { }
