namespace ViaTradeBackend.Models.Exceptions;

public class ForbiddenException(string message) : Exception(message)
{
}
