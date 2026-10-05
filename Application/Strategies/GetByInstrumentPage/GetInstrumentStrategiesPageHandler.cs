using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Common.Abstractions.Repositories;
using ViaTrade.Application.Common.Exceptions;
using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Strategies.Common;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Strategies.GetByInstrumentPage;

public sealed class GetInstrumentStrategiesPageHandler(
	IReadRepository<Instrument> instrumentRepository,
	IReadRepository<UserStrategyInstrument> userStrategyInstrumentRepository
) : IQueryHandler<GetInstrumentStrategiesPageQuery, PageResult<StrategySubscriptionResult>>
{
	public async Task<PageResult<StrategySubscriptionResult>> HandleAsync(
		GetInstrumentStrategiesPageQuery query,
		CancellationToken ct = default
	)
	{
		var instrumentExists = await instrumentRepository.AnyAsync(instrument => instrument.Id == query.InstrumentId, ct);

		if (!instrumentExists)
			throw new NotFoundException("Instrument not found.", "instrument_not_found");

		var specification = new InstrumentStrategiesPageSpecification(
			query.UserId,
			query.InstrumentId,
			query.StrategyFilter,
			query.PageOptions,
			query.StrategySort
		);
		return await userStrategyInstrumentRepository.GetPageAsync(
			specification,
			StrategySubscriptionResult.LinkProjection(query.UserId),
			ct
		);
	}
}
