using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Strategies.Interfaces;
using Domain.Entities;

namespace Application.Strategies;

public class StrategyCommandService(
	IUserStrategyInstrumentRepository userStrategyInstrumentRepository,
	IUserStrategyRepository userStrategyRepository,
	IUnitOfWork uow
) : IStrategyCommandService
{
	public async Task LinkInstrumentAsync(int userId, int strategyId, int instrumentId, CancellationToken ct)
	{
		var strategyExists = await userStrategyRepository.ExistsAsync(
			strategy => strategy.UserId == userId && strategy.StrategyId == strategyId,
			ct
		);
		if (!strategyExists)
			throw new NotFoundException("Strategy not found.", "strategy_not_found");

		var strategyCodeExists = await userStrategyInstrumentRepository.ExistsAsync(
			strategyCode =>
				strategyCode.UserId == userId
				&& strategyCode.StrategyId == strategyId
				&& strategyCode.InstrumentId == instrumentId,
			ct
		);
		if (strategyCodeExists)
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
		var strategyExists = await userStrategyRepository.ExistsAsync(
			strategy => strategy.UserId == userId && strategy.StrategyId == strategyId,
			ct
		);
		if (strategyExists)
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
