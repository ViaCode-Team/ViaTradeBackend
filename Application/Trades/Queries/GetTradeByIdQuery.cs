using Application.Common.Interfaces;
using Application.Trades.Interfaces;
using Domain.Trades.Entities;
using MediatR;

namespace Application.Trades.Queries;

public record GetTradeByIdQuery(int Id, int UserId) : IQuery<Trade>;

public class GetTradeByIdQueryHandler(ITradeRepository tradeRepository)
	: IRequestHandler<GetTradeByIdQuery, Trade>
{
	public async Task<Trade> Handle(GetTradeByIdQuery request, CancellationToken ct)
	{
		var trade = await tradeRepository.FirstOrDefaultAsync(x => x.Id == request.Id && x.UserId == request.UserId, ct);
		if (trade == null)
			throw new KeyNotFoundException();

		return trade;
	}
}
