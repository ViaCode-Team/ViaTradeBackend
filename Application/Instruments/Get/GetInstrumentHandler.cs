using Mediator;
using ViaTrade.Application.Common.Abstractions.Repositories;
using ViaTrade.Application.Common.Exceptions;
using ViaTrade.Application.Instruments.Common;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Instruments.Get;

public sealed class GetInstrumentHandler(IReadRepository<Instrument> instrumentRepository)
	: IQueryHandler<GetInstrumentQuery, InstrumentResult>
{
	public async ValueTask<InstrumentResult> Handle(GetInstrumentQuery query, CancellationToken ct)
	{
		return await instrumentRepository.FirstOrDefaultAsync(
				instrument => instrument.Id == query.InstrumentId,
				InstrumentResult.Projection,
				ct
			) ?? throw new NotFoundException("Instrument not found.", "instrument_not_found");
	}
}
