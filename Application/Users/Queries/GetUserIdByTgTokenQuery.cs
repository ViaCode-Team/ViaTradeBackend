using Application.Common.Interfaces;
using Application.Users.Interfaces;
using MediatR;

namespace Application.Users.Queries;

public record GetUserIdByTgTokenQuery(string TgToken) : IQuery<int?>;

public class GetUserIdByTgTokenQueryHandler(ITgTokenRepository tgTokenRepository)
	: IRequestHandler<GetUserIdByTgTokenQuery, int?>
{
	public async Task<int?> Handle(GetUserIdByTgTokenQuery request, CancellationToken ct)
	{
		var userId = await tgTokenRepository.GetUserIdAsync(request.TgToken);

		if (userId == null)
			return null;

		await tgTokenRepository.RemoveAsync(request.TgToken);

		return userId;
	}
}
