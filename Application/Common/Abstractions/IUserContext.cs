namespace ViaTrade.Application.Common.Abstractions;

public interface IUserContext
{
	int UserId { get; }

	string SessionId { get; }
}
