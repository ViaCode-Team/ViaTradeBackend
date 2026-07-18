namespace Application.Auth.Models;

public class TgTokenCreateDto
{
	public required string TgToken { get; set; }

	public required string TgId { get; set; }
}
