using ViaTrade.Application.Common.Interfaces;
using ViaTrade.Application.Common.Interfaces.Repositories;
using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Strategies.Models;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Strategies.Interfaces;

public interface IUserStrategyInstrumentRepository : IRepository<UserStrategyInstrument>
{
	Task<PageResult<StrategySubscriptionDto>> GetStrategiesPageByInstrumentAsync(
		int userId,
		IQueryObject<UserStrategyInstrument> queryObject,
		PageOptions pageOptions,
		CancellationToken ct = default
	);
	Task<StrategyInstrumentsPageResult> GetInstrumentsPageByStrategyAsync(
		int strategyId,
		IQueryObject<UserStrategyInstrument> queryObject,
		PageOptions pageOptions,
		CancellationToken ct = default
	);
}
