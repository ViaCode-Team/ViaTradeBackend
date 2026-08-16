using System.ComponentModel.DataAnnotations;

namespace ViaTrade.Application.Users.Models;

public class UserInternalDto
{
	public required int Id { get; set; }

	public required string Login { get; set; }

	public required string PasswordHash { get; set; }

	public DateTime LastLoginAt { get; set; } = DateTime.UtcNow;

	public required DateTime RegisteredAt { get; set; }

	[MaxLength(512)]
	public string? TelegramId { get; set; }
}
