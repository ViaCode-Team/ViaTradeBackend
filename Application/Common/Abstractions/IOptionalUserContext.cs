namespace ViaTrade.Application.Common.Abstractions;

public interface IOptionalUserContext
{
	int? UserId { get; }

	string? SessionId { get; }
}
