using System.ComponentModel.DataAnnotations;

namespace Application.Common.Models;

public class UserInternalDto
{
	public required int Id { get; set; }

	public required string Login { get; set; }

	public required string HashPassword { get; set; }

	public DateTime LastLoginDate { get; set; } = DateTime.UtcNow;

	public required DateTime RegisterDate { get; set; }

	[MaxLength(512)]
	public string? TelegramId { get; set; }
}
