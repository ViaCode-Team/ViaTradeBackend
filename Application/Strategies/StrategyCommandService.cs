using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Strategies.Interfaces;
using Domain.Strategies.Entities;

namespace Application.Strategies;

public class StrategyCommandService(
	IUserStrategyTradeCodeRepository userStrategyTradeCodeRepository,
	IUserTradeStrategyRepository userTradeStrategyRepository,
	IUnitOfWork uow
) : IStrategyCommandService
{
	public async Task LinkInstrumentAsync(int userId, int strategyId, int instrumentId, CancellationToken ct)
	{
		var strategyExists = await userTradeStrategyRepository.ExistsAsync(
			strategy => strategy.UserId == userId && strategy.TradeStrategyId == strategyId,
			ct
		);
		if (!strategyExists)
			throw new NotFoundException("Strategy not found.", "strategy_not_found");

		var strategyCodeExists = await userStrategyTradeCodeRepository.ExistsAsync(
			strategyCode =>
				strategyCode.UserId == userId
				&& strategyCode.StrategyId == strategyId
				&& strategyCode.TradeCodeId == instrumentId,
			ct
		);
		if (strategyCodeExists)
			return;

		var strategyCode = new UserStrategyTradeCode
		{
			UserId = userId,
			StrategyId = strategyId,
			TradeCodeId = instrumentId,
		};

		await userStrategyTradeCodeRepository.AddAsync(strategyCode, ct);
		await uow.SaveChangesAsync(ct);
	}

	public async Task ActivateAsync(int userId, int strategyId, CancellationToken ct)
	{
		var strategyExists = await userTradeStrategyRepository.ExistsAsync(
			strategy => strategy.UserId == userId && strategy.TradeStrategyId == strategyId,
			ct
		);
		if (strategyExists)
			return;

		var strategyLink = new UserTradeStrategy { UserId = userId, TradeStrategyId = strategyId };

		await userTradeStrategyRepository.AddAsync(strategyLink, ct);
		await uow.SaveChangesAsync(ct);
	}

	public async Task UnlinkInstrumentAsync(int userId, int strategyId, int instrumentId, CancellationToken ct)
	{
		var affectedRows = await userStrategyTradeCodeRepository.ExecuteDeleteAsync(
			e => e.UserId == userId && e.StrategyId == strategyId && e.TradeCodeId == instrumentId,
			ct
		);

		if (affectedRows == 0)
			throw new NotFoundException("User strategy code not found.", "strategy_code_not_found");
	}

	public async Task DeactivateAsync(int userId, int strategyId, CancellationToken ct)
	{
		var affectedRows = await userTradeStrategyRepository.ExecuteDeleteAsync(
			e => e.UserId == userId && e.TradeStrategyId == strategyId,
			ct
		);

		if (affectedRows == 0)
			throw new NotFoundException("User strategy not found.", "user_strategy_not_found");
	}
}
