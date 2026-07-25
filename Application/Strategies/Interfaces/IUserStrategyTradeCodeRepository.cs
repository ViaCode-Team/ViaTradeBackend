using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using Application.Strategies.Models;
using Application.TradeCodes.Models;
using Domain.Strategies.Entities;

namespace Application.Strategies.Interfaces;

public interface IUserStrategyTradeCodeRepository : IRepository<UserStrategyTradeCode>
{
	Task<PageResult<UserStrategyTradeCode>> GetPageByUserAsync(
		int userId,
		PageOptions pageOptions,
		CancellationToken ct = default
	);
	Task<PageResult<RelatedTradeStrategyDto>> GetStrategiesPageByTradeCodeAsync(
		int userId,
		int tradeCodeId,
		StrategyFilter strategyFilter,
		StrategySort strategySort,
		PageOptions pageOptions,
		CancellationToken ct = default
	);
	Task<PageResult<RelatedTradeCodeDto>> GetTradeCodesPageByStrategyAsync(
		int userId,
		int strategyId,
		TradeCodeSort tradeCodeSort,
		PageOptions pageOptions,
		CancellationToken ct = default
	);
}
