using Application.Common.Interfaces;
using Application.Statistics.Models;
using Application.Strategies.Interfaces;
using MediatR;

namespace Application.Strategies.Queries;

public record GetStrategyStatisticQuery(int UserId) : IQuery<StrategyStatisticReadModel>;

public class GetStrategyStatisticQueryHandler(
	ITradeStrategyRepository tradeStrategyRepository,
	IUserTradeStrategyRepository userTradeStrategyRepository)
	: IRequestHandler<GetStrategyStatisticQuery, StrategyStatisticReadModel>
{
	public async Task<StrategyStatisticReadModel> Handle(GetStrategyStatisticQuery request, CancellationToken ct)
	{
		var totalStrategiesTask = tradeStrategyRepository.CountAsync(ct);
		var activeStrategiesTask = userTradeStrategyRepository.CountByUserAsync(request.UserId, ct);

		await Task.WhenAll(totalStrategiesTask, activeStrategiesTask);

		var totalStrategies = await totalStrategiesTask;
		var activeStrategies = await activeStrategiesTask;

		return new StrategyStatisticReadModel
		{
			TotalStrategies = totalStrategies,
			ActiveStrategies = activeStrategies,
			DisabledStrategies = Math.Max(totalStrategies - activeStrategies, 0)
		};
	}
}
