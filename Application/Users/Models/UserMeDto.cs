namespace Application.Users.Models;

public class UserMeDto
{
	public required int Id { get; set; }

	public required string Login { get; set; }

	public required DateTime LastLoginAt { get; set; }

	public required DateTime RegisteredAt { get; set; }

	public string? TelegramId { get; set; }
}
