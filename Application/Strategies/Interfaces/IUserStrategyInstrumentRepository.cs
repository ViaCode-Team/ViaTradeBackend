using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using Application.Instruments.Models;
using Application.Strategies.Models;
using Domain.Entities;

namespace Application.Strategies.Interfaces;

public interface IUserStrategyInstrumentRepository : IRepository<UserStrategyInstrument>
{
	Task<PageResult<Strategy>> GetStrategiesPageByInstrumentAsync(
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
