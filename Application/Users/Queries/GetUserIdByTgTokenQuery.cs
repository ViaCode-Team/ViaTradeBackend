using Application.Interfaces.Repositories.Redis;
using Domain.Entities.Redis;
using MediatR;

namespace Application.Users.Queries;

public record GetUserIdByTgTokenQuery(string TgToken) : IRequest<int?>;

public class GetUserIdByTgTokenQueryHandler(IRedisRepository<TgTokenEntity> tgTokenRepository) 
	: IRequestHandler<GetUserIdByTgTokenQuery, int?>
{
	private readonly IRedisRepository<TgTokenEntity> _tgTokenRepository = tgTokenRepository;

	public async Task<int?> Handle(GetUserIdByTgTokenQuery request, CancellationToken cancellationToken)
	{
		var entity = await _tgTokenRepository.GetAsync(request.TgToken);

		if (entity == null)
			return null;

		await _tgTokenRepository.RemoveAsync(request.TgToken);

		return entity.UserId;
	}
}
