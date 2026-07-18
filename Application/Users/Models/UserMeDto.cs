namespace Application.Users.Models;

public class UserMeDto
{
	public required int Id { get; set; }

	public required string Login { get; set; }

	public required DateTime LastLoginDate { get; set; }

	public required DateTime RegisterDate { get; set; }

	public string? TgId { get; set; }
}
