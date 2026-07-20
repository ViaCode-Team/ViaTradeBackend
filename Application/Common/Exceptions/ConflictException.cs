namespace Application.Common.Exceptions;

public class ConflictException(string message, string code = "conflict") : AppException(message, code) { }
