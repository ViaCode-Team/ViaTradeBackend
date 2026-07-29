namespace Application.Users.Models;

public class UserSessionDto
{
	public required string Id { get; set; }

	public required int UserId { get; set; }

	public required string UserAgent { get; set; }

	public DateTime CreatedAt { get; set; }

	public DateTime LastSeen { get; set; }

	public DateTime ExpiresAt { get; set; }
}
