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

	public async Task ActivateAsync(int userId, int strategyId, CancellationToken ct)
	{
		var strategy = await strategyRepository.FindWithActivityAsync(userId, strategyId, ct);
		if (strategy == null)
			throw new NotFoundException("Strategy not found.", "strategy_not_found");

		if (strategy.IsActive)
			return;

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

	public async Task DeactivateAsync(int userId, int strategyId, CancellationToken ct)
	{
		var affectedRows = await userStrategyRepository.ExecuteDeleteAsync(
			e => e.UserId == userId && e.StrategyId == strategyId,
			ct
		);

		if (affectedRows == 0)
			throw new NotFoundException("User strategy not found.", "user_strategy_not_found");
	}
}
