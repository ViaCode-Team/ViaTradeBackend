using Application.Strategies.Interfaces;
using Domain.Strategies.Entities;
using MediatR;

namespace Application.Strategies.Queries;

public record GetStrategyByIdQuery(int StrategyId) : IRequest<TradeStrategy>;

public class GetStrategyByIdQueryHandler(ITradeStrategyRepository tradeStrategyRepository)
	: IRequestHandler<GetStrategyByIdQuery, TradeStrategy>
{
	private readonly ITradeStrategyRepository _tradeStrategyRepository = tradeStrategyRepository;

	public async Task<TradeStrategy> Handle(GetStrategyByIdQuery request, CancellationToken cancellationToken)
	{
		return await _tradeStrategyRepository.GetByIdAsync(request.StrategyId, cancellationToken)
			?? throw new KeyNotFoundException();
	}
}
