using Application.Common.Interfaces;
using Application.Users.Interfaces;
using MediatR;

namespace Application.Users.Queries;

public record GetUserIdByTgTokenQuery(string TgToken) : IQuery<int?>;

public class GetUserIdByTgTokenQueryHandler(ITgTokenRepository tgTokenRepository)
	: IRequestHandler<GetUserIdByTgTokenQuery, int?>
{
	private readonly ITgTokenRepository _tgTokenRepository = tgTokenRepository;

	public async Task<int?> Handle(GetUserIdByTgTokenQuery request, CancellationToken cancellationToken)
	{
		var userId = await _tgTokenRepository.GetUserIdAsync(request.TgToken);

		if (userId == null)
			return null;

		await _tgTokenRepository.RemoveAsync(request.TgToken);

		return userId;
	}
}
