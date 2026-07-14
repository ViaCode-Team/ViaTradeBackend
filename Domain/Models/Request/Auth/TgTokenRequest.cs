namespace Domain.Models.Request.Auth;

public class TgTokenRequest
{
	public required string TgToken { get; set; }

	public required string TgId { get; set; }
}
