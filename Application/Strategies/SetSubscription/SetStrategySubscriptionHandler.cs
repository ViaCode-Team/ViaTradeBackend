using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Common.Abstractions.Repositories;
using ViaTrade.Application.Common.Exceptions;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Strategies.SetSubscription;

public sealed class SetStrategySubscriptionHandler(
	IRepository<UserStrategy> userStrategyRepository,
	IReadRepository<Strategy> strategyRepository,
	IUnitOfWork uow
) : ICommandHandler<SetStrategySubscriptionCommand>
{
	public async Task HandleAsync(SetStrategySubscriptionCommand command, CancellationToken ct = default)
	{
		var strategyExists = await strategyRepository.AnyAsync(strategy => strategy.Id == command.StrategyId, ct);
		if (!strategyExists)
			throw new NotFoundException("Strategy not found.", "strategy_not_found");

		if (!command.IsSubscribed)
		{
			await userStrategyRepository.ExecuteDeleteAsync(
				link => link.UserId == command.UserId && link.StrategyId == command.StrategyId,
				ct
			);
			return;
		}

		var strategyLink = new UserStrategy { UserId = command.UserId, StrategyId = command.StrategyId };

		userStrategyRepository.Add(strategyLink);
		await uow.SaveChangesAsync(ct);
	}
}
