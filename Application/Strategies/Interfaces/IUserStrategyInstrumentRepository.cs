using ViaTrade.Application.Common.Interfaces.Repositories;
using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Instruments.Models;
using ViaTrade.Application.Strategies.Models;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Strategies.Interfaces;

public interface IUserStrategyInstrumentRepository : IRepository<UserStrategyInstrument>
{
	Task<PageResult<StrategySubscriptionDto>> GetStrategiesPageByInstrumentAsync(
		int userId,
		int instrumentId,
		StrategyFilter strategyFilter,
		StrategySort strategySort,
		PageOptions pageOptions,
		CancellationToken ct = default
	);
	Task<StrategyInstrumentsPageResult> GetInstrumentsPageByStrategyAsync(
		int userId,
		int strategyId,
		StrategyInstrumentFilter instrumentFilter,
		InstrumentSort instrumentSort,
		PageOptions pageOptions,
		CancellationToken ct = default
	);
}
