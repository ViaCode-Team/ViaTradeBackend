using Application.Interfaces.Repositories.Database;
using Application.Interfaces.Repositories.Redis;
using Domain.Entities.Redis;
using MediatR;

namespace Application.Users.Commands;

public record LinkTelegramCommand(string TgToken, string TgId) : IRequest;

public class LinkTelegramCommandHandler(
	IUserRepository userRepository,
	IRedisRepository<TgTokenEntity> tgTokenRepository) 
	: IRequestHandler<LinkTelegramCommand>
{
	private readonly IUserRepository _userRepository = userRepository;
	private readonly IRedisRepository<TgTokenEntity> _tgTokenRepository = tgTokenRepository;

	public async Task Handle(LinkTelegramCommand request, CancellationToken cancellationToken)
	{
		var entity = await _tgTokenRepository.GetAsync(request.TgToken);
		if (entity == null)
			throw new NullReferenceException(nameof(request.TgToken));

		await _tgTokenRepository.RemoveAsync(request.TgToken);
		var userId = entity.UserId;

		var affectedRows = await _userRepository.UpdateTgIdAsync(userId, request.TgId, cancellationToken);
		if (affectedRows == 0)
			throw new NullReferenceException(nameof(request.TgToken));
	}
}
