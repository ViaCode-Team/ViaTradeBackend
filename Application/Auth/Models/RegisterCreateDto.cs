namespace Application.Auth.Models;

public class RegisterCreateDto
{
	public required string Login { get; set; }

	public required string Password { get; set; }
}
