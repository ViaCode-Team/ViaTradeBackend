namespace Application.Contracts.Dto.Requests.Auth;

public class TgTokenCreateDto
{
	public required string TgToken { get; set; }

	public required string TgId { get; set; }
}
