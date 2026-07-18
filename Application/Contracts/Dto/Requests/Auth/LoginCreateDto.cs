namespace Application.Contracts.Dto.Requests.Auth;

public record LoginCreateDto
{
	public required string Login { get; set; }

	public required string Password { get; set; }
}
