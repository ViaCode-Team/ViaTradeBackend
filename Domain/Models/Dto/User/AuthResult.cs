namespace Domain.Models.Dto.User;

public class AuthResult
{
	public required string AccessToken { get; set; }
	public required string RefreshToken { get; set; }
}
