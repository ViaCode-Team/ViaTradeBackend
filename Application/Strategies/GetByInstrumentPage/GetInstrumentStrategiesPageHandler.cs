using Mediator;
using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Common.Abstractions.Repositories;
using ViaTrade.Application.Common.Exceptions;
using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Strategies.Common;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Strategies.GetByInstrumentPage;

public sealed class GetInstrumentStrategiesPageHandler(
	IUserContext userContext,
	IReadRepository<Instrument> instrumentRepository,
	IReadRepository<UserStrategyInstrument> userStrategyInstrumentRepository
) : IQueryHandler<GetInstrumentStrategiesPageQuery, PageResult<StrategySubscriptionResult>>
{
	public async ValueTask<PageResult<StrategySubscriptionResult>> Handle(
		GetInstrumentStrategiesPageQuery query,
		CancellationToken ct
	)
	{
		var instrumentExists = await instrumentRepository.AnyAsync(
			instrument => instrument.Id == query.InstrumentId,
			ct
		);

		if (!instrumentExists)
			throw new NotFoundException("Instrument not found.", "instrument_not_found");

		var specification = new InstrumentStrategiesPageSpecification(
			userContext.UserId,
			query.InstrumentId,
			query.StrategyFilter,
			query.PageOptions,
			query.StrategySort
		);

		return await userStrategyInstrumentRepository.GetPageAsync(
			specification,
			StrategySubscriptionResult.LinkProjection(userContext.UserId),
			ct
		);
	}
}
