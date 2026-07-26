using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using Application.Strategies.Models;
using Application.TradeCodes.Models;
using Domain.Strategies.Entities;

namespace Application.Strategies.Interfaces;

public interface IUserStrategyTradeCodeRepository : IRepository<UserStrategyTradeCode>
{
	Task<PageResult<TradeStrategy>> GetStrategiesPageByInstrumentAsync(
		int userId,
		int instrumentId,
		StrategyFilter strategyFilter,
		StrategySort strategySort,
		PageOptions pageOptions,
		CancellationToken ct = default
	);
	Task<PageResult<RelatedInstrumentDto>> GetInstrumentsPageByStrategyAsync(
		int userId,
		int strategyId,
		InstrumentSort instrumentSort,
		PageOptions pageOptions,
		CancellationToken ct = default
	);
}
