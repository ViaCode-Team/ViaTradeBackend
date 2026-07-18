using Application.Interfaces.Repositories.Database;
using Domain.Trades.Entities;
using MediatR;

namespace Application.Trades.Queries;

public record GetTradeByIdQuery(int Id, int UserId) : IRequest<Trade>;

public class GetTradeByIdQueryHandler(ITradeRepository tradeRepository) 
	: IRequestHandler<GetTradeByIdQuery, Trade>
{
	private readonly ITradeRepository _tradeRepository = tradeRepository;

	public async Task<Trade> Handle(GetTradeByIdQuery request, CancellationToken cancellationToken)
	{
		var trade = await _tradeRepository.GetByIdAsync(request.Id, cancellationToken);
		if (trade == null || trade.UserId != request.UserId)
			throw new KeyNotFoundException();

		return trade;
	}
}
