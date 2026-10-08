using Mediator;
using ViaTrade.Application.Common.Abstractions.Repositories;
using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Instruments.Common;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Instruments.GetPage;

public sealed class GetInstrumentsPageHandler(IReadRepository<Instrument> instrumentRepository)
	: IQueryHandler<GetInstrumentsPageQuery, PageResult<InstrumentResult>>
{
	public async ValueTask<PageResult<InstrumentResult>> Handle(GetInstrumentsPageQuery query, CancellationToken ct)
	{
		var specification = new InstrumentsPageSpecification(
			query.InstrumentFilter,
			query.InstrumentSearch,
			query.PageOptions,
			query.InstrumentSort
		);

		return await instrumentRepository.GetPageAsync(specification, InstrumentResult.Projection, ct);
	}
}
