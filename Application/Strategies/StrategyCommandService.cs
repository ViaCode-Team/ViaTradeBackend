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
	public async Task CreateCodeAsync(int userId, int strategyId, int tradeCodeId, CancellationToken ct)
	{
		var strategyCode = new UserStrategyTradeCode
		{
			UserId = userId,
			StrategyId = strategyId,
			TradeCodeId = tradeCodeId,
		};

		await userStrategyTradeCodeRepository.AddAsync(strategyCode, ct);
		await uow.SaveChangesAsync(ct);
	}

	public async Task CreateAsync(int userId, int strategyId, CancellationToken ct)
	{
		var strategyLink = new UserTradeStrategy { UserId = userId, TradeStrategyId = strategyId };

		await userTradeStrategyRepository.AddAsync(strategyLink, ct);
		await uow.SaveChangesAsync(ct);
	}

	public async Task DeleteCodeAsync(int userId, int strategyId, int tradeCodeId, CancellationToken ct)
	{
		var affectedRows = await userStrategyTradeCodeRepository.ExecuteDeleteAsync(
			e => e.UserId == userId && e.StrategyId == strategyId && e.TradeCodeId == tradeCodeId,
			ct
		);

		if (affectedRows == 0)
			throw new NotFoundException("User strategy code not found.", "strategy_code_not_found");
	}

	public async Task DeleteAsync(int userId, int strategyId, CancellationToken ct)
	{
		var affectedRows = await userTradeStrategyRepository.ExecuteDeleteAsync(
			e => e.UserId == userId && e.TradeStrategyId == strategyId,
			ct
		);

		if (affectedRows == 0)
			throw new NotFoundException("User strategy not found.", "user_strategy_not_found");
	}
}
