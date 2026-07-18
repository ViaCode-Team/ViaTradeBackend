namespace Application.Models;

public class AuthInternalResult
{
	public required string AccessToken { get; set; }

	public required string RefreshToken { get; set; }
}
