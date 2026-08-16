namespace ViaTrade.Application.Common.Exceptions;

public class ConflictException(string message, string code = "conflict", Exception? innerException = null)
	: AppException(message, code, innerException) { }
