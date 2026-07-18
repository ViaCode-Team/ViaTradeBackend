using Application.Common.Interfaces;
using Application.Trades.Interfaces;
using Domain.Trades.Entities;
using MediatR;

namespace Application.Trades.Queries;

public record GetTradeByIdQuery(int Id, int UserId) : IQuery<Trade>;

public class GetTradeByIdQueryHandler(ITradeRepository tradeRepository)
	: IRequestHandler<GetTradeByIdQuery, Trade>
{
	private readonly ITradeRepository _tradeRepository = tradeRepository;

	public async Task<Trade> Handle(GetTradeByIdQuery request, CancellationToken cancellationToken)
	{
		var trade = await _tradeRepository.FirstOrDefaultAsync(x => x.Id == request.Id && x.UserId == request.UserId, cancellationToken);
		if (trade == null)
			throw new KeyNotFoundException();

		return trade;
	}
}
