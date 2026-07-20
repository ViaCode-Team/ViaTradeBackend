namespace Application.Common.Exceptions;

public class BadRequestException(string message, string code = "bad_request") : AppException(message, code) { }
