using ViaTrade.Application.Common.Exceptions;
using ViaTrade.Application.Common.Interfaces;
using ViaTrade.Application.Strategies.Interfaces;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Strategies;

public class StrategyCommandService(
	IUserStrategyInstrumentRepository userStrategyInstrumentRepository,
	IUserStrategyRepository userStrategyRepository,
	IStrategyRepository strategyRepository,
	IUnitOfWork uow
) : IStrategyCommandService
{
	public async Task LinkInstrumentAsync(int userId, int strategyId, int instrumentId, CancellationToken ct)
	{
		var linkState = await strategyRepository.FindInstrumentLinkStateAsync(userId, strategyId, instrumentId, ct);
		if (linkState == null)
			throw new NotFoundException("Strategy not found.", "strategy_not_found");

		if (!linkState.InstrumentExists)
			throw new NotFoundException("Instrument not found.", "instrument_not_found");

		if (linkState.LinkExists)
			return;

		var strategyCode = new UserStrategyInstrument
		{
			UserId = userId,
			StrategyId = strategyId,
			InstrumentId = instrumentId,
		};

		await userStrategyInstrumentRepository.AddAsync(strategyCode, ct);
		await uow.SaveChangesAsync(ct);
	}

	public async Task SetSubscriptionAsync(int userId, int strategyId, bool isSubscribed, CancellationToken ct)
	{
		var strategyExists = await strategyRepository.ExistsAsync(strategy => strategy.Id == strategyId, ct);
		if (!strategyExists)
			throw new NotFoundException("Strategy not found.", "strategy_not_found");

		if (!isSubscribed)
		{
			await userStrategyRepository.ExecuteUnsubscribeAsync(userId, strategyId, ct);
			return;
		}

		var strategyLink = new UserStrategy { UserId = userId, StrategyId = strategyId };

		await userStrategyRepository.AddAsync(strategyLink, ct);
		await uow.SaveChangesAsync(ct);
	}

	public async Task UnlinkInstrumentAsync(int userId, int strategyId, int instrumentId, CancellationToken ct)
	{
		var affectedRows = await userStrategyInstrumentRepository.ExecuteDeleteAsync(
			e => e.UserId == userId && e.StrategyId == strategyId && e.InstrumentId == instrumentId,
			ct
		);

		if (affectedRows == 0)
			throw new NotFoundException("User strategy code not found.", "strategy_code_not_found");
	}
}
