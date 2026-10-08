using Mediator;
using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Common.Abstractions.Repositories;
using ViaTrade.Application.Common.Exceptions;
using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Instruments.Common;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Strategies.GetInstrumentsPage;

public sealed class GetStrategyInstrumentsPageHandler(
	IUserContext userContext,
	IReadRepository<Strategy> strategyRepository,
	IReadRepository<UserStrategyInstrument> userStrategyInstrumentRepository
) : IQueryHandler<GetStrategyInstrumentsPageQuery, PageResult<InstrumentResult>>
{
	public async ValueTask<PageResult<InstrumentResult>> Handle(
		GetStrategyInstrumentsPageQuery query,
		CancellationToken ct
	)
	{
		var strategyExists = await strategyRepository.AnyAsync(strategy => strategy.Id == query.StrategyId, ct);
		if (!strategyExists)
			throw new NotFoundException("Strategy not found.", "strategy_not_found");

		var specification = new StrategyInstrumentsPageSpecification(
			userContext.UserId,
			query.StrategyId,
			query.InstrumentFilter,
			query.PageOptions,
			query.InstrumentSort
		);

		return await userStrategyInstrumentRepository.GetPageAsync(specification, InstrumentResult.LinkProjection, ct);
	}
}
