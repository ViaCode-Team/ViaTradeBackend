using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Strategies.Interfaces;
using Domain.Entities;

namespace Application.Strategies;

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

	public async Task SetActivityAsync(int userId, int strategyId, bool isActive, CancellationToken ct)
	{
		var currentActivity = await strategyRepository.FindActivityAsync(userId, strategyId, ct);
		if (currentActivity == null)
			throw new NotFoundException("Strategy not found.", "strategy_not_found");

		if (currentActivity.Value == isActive)
			return;

		if (isActive)
		{
			var strategyLink = new UserStrategy { UserId = userId, StrategyId = strategyId };

			await userStrategyRepository.AddAsync(strategyLink, ct);
			await uow.SaveChangesAsync(ct);
			return;
		}

		await userStrategyRepository.ExecuteDeleteAsync(
			e => e.UserId == userId && e.StrategyId == strategyId,
			ct
		);
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
