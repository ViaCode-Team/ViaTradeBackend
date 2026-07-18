namespace Application.Contracts.Dto.Requests.Auth;

public class RegisterCreateDto
{
	public required string Login { get; set; }

	public required string Password { get; set; }
}
