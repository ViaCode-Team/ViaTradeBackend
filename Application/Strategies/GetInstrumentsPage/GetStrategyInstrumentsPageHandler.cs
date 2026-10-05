using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Common.Abstractions.Repositories;
using ViaTrade.Application.Common.Exceptions;
using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Instruments.Common;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Strategies.GetInstrumentsPage;

public sealed class GetStrategyInstrumentsPageHandler(
	IReadRepository<Strategy> strategyRepository,
	IReadRepository<UserStrategyInstrument> userStrategyInstrumentRepository
) : IQueryHandler<GetStrategyInstrumentsPageQuery, PageResult<InstrumentResult>>
{
	public async Task<PageResult<InstrumentResult>> HandleAsync(
		GetStrategyInstrumentsPageQuery query,
		CancellationToken ct = default
	)
	{
		var strategyExists = await strategyRepository.AnyAsync(strategy => strategy.Id == query.StrategyId, ct);
		if (!strategyExists)
			throw new NotFoundException("Strategy not found.", "strategy_not_found");

		var specification = new StrategyInstrumentsPageSpecification(
			query.UserId,
			query.StrategyId,
			query.InstrumentFilter,
			query.PageOptions,
			query.InstrumentSort
		);
		return await userStrategyInstrumentRepository.GetPageAsync(
			specification,
			InstrumentResult.LinkProjection,
			ct
		);
	}
}
