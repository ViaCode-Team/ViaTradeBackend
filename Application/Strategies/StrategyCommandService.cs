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
		bool isUserStrategyCodeExist = await userStrategyTradeCodeRepository.ExistsAsync(
			e => e.UserId == userId && e.StrategyId == strategyId && e.TradeCodeId == tradeCodeId,
			ct
		);

		if (isUserStrategyCodeExist)
			throw new InvalidOperationException("User strategy code already exists");

		var newUserStrategyCode = new UserStrategyTradeCode
		{
			UserId = userId,
			TradeCodeId = tradeCodeId,
			StrategyId = strategyId,
		};

		await userStrategyTradeCodeRepository.AddAsync(newUserStrategyCode, ct);
		await uow.SaveChangesAsync(ct);
	}

	public async Task CreateAsync(int userId, int strategyId, CancellationToken ct)
	{
		var isUserExist = await userTradeStrategyRepository.ExistsAsync(
			e => e.UserId == userId && e.TradeStrategyId == strategyId,
			ct
		);

		if (isUserExist)
			throw new InvalidOperationException("User strategy already exists");

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
			throw new KeyNotFoundException("User strategy code not found");
	}

	public async Task DeleteAsync(int userId, int strategyId, CancellationToken ct)
	{
		var affectedRows = await userTradeStrategyRepository.ExecuteDeleteAsync(
			e => e.UserId == userId && e.TradeStrategyId == strategyId,
			ct
		);

		if (affectedRows == 0)
			throw new KeyNotFoundException("User strategy not found");
	}
}
