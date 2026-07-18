using Application.Interfaces.Repositories.Database;
using Application.Models.Statistic;
using MediatR;

namespace Application.Strategies.Queries;

public record GetStrategyStatisticQuery(int UserId) : IRequest<StrategyStatisticReadModel>;

public class GetStrategyStatisticQueryHandler(
	ITradeStrategyRepository tradeStrategyRepository,
	IUserTradeStrategyRepository userTradeStrategyRepository) 
	: IRequestHandler<GetStrategyStatisticQuery, StrategyStatisticReadModel>
{
	private readonly ITradeStrategyRepository _tradeStrategyRepository = tradeStrategyRepository;
	private readonly IUserTradeStrategyRepository _userTradeStrategyRepository = userTradeStrategyRepository;

	public async Task<StrategyStatisticReadModel> Handle(GetStrategyStatisticQuery request, CancellationToken cancellationToken)
	{
		var totalStrategiesTask = _tradeStrategyRepository.CountAsync(cancellationToken);
		var activeStrategiesTask = _userTradeStrategyRepository.CountByUserAsync(request.UserId, cancellationToken);

		await Task.WhenAll(totalStrategiesTask, activeStrategiesTask);

		var totalStrategies = totalStrategiesTask.Result;
		var activeStrategies = activeStrategiesTask.Result;

		return new StrategyStatisticReadModel
		{
			TotalStrategies = totalStrategies,
			ActiveStrategies = activeStrategies,
			DisabledStrategies = Math.Max(totalStrategies - activeStrategies, 0)
		};
	}
}
