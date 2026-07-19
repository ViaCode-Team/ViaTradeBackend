using Application.Common.Interfaces;
using Application.Strategies.Interfaces;
using Domain.Strategies.Entities;
using MediatR;

namespace Application.Strategies.Queries;

public record GetStrategyByIdQuery(int StrategyId) : IQuery<TradeStrategy>;

public class GetStrategyByIdQueryHandler(ITradeStrategyRepository tradeStrategyRepository)
	: IRequestHandler<GetStrategyByIdQuery, TradeStrategy>
{
	public async Task<TradeStrategy> Handle(GetStrategyByIdQuery request, CancellationToken ct)
	{
		return await tradeStrategyRepository.GetByIdAsync(request.StrategyId, ct)
			?? throw new KeyNotFoundException();
	}
}
