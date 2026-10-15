namespace ViaTrade.Application.Common.Exceptions;

public sealed class RateLimitException() : AppException("Too many requests. Try again later.", "rate_limit_exceeded");
