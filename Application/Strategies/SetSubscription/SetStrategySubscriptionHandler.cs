using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Common.Abstractions.Repositories;
using ViaTrade.Application.Common.Exceptions;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Strategies.SetSubscription;

public sealed class SetStrategySubscriptionHandler(
	IUserContext userContext,
	IRepository<UserStrategy> userStrategyRepository,
	IReadRepository<Strategy> strategyRepository,
	IUnitOfWork uow
) : IVoidCommandHandler<SetStrategySubscriptionCommand>
{
	public async ValueTask Handle(SetStrategySubscriptionCommand command, CancellationToken ct)
	{
		var strategyExists = await strategyRepository.AnyAsync(strategy => strategy.Id == command.StrategyId, ct);
		if (!strategyExists)
			throw new NotFoundException("Strategy not found.", "strategy_not_found");

		if (command.IsSubscribed == false)
		{
			await userStrategyRepository.ExecuteDeleteAsync(
				link => link.UserId == userContext.UserId && link.StrategyId == command.StrategyId,
				ct
			);
			return;
		}

		var strategyLink = new UserStrategy { UserId = userContext.UserId, StrategyId = command.StrategyId };

		userStrategyRepository.Add(strategyLink);
		await uow.SaveChangesAsync(ct);
	}
}
