using Domain.Users.Entities;
namespace Application.Contracts.Dto.User;

public class UserTgDto
{
	public int Id { get; set; }

	public required string TgId { get; set; }
}
