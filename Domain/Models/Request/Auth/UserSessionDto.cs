namespace Domain.Models;

public class UserSessionDto
{
	public required string Id { get; set; }

	public required string UserAgent { get; set; }

	public required DateTime CreatedAt { get; set; }

	public required DateTime LastSeen { get; set; }
}
