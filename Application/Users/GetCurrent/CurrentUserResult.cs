using System.Linq.Expressions;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Users.GetCurrent;

public sealed record CurrentUserResult
{
	public required int Id { get; init; }

	public required string Login { get; init; }

	public required DateTime LastLoginAt { get; init; }

	public required DateTime RegisteredAt { get; init; }

	public string? TelegramId { get; init; }

	public static Expression<Func<User, CurrentUserResult>> Projection { get; } =
		user => new CurrentUserResult
		{
			Id = user.Id,
			Login = user.Login,
			LastLoginAt = user.LastLoginAt,
			RegisteredAt = user.RegisteredAt,
			TelegramId = user.TelegramId,
		};
}
