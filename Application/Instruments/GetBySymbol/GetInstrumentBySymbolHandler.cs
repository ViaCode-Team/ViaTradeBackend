using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Common.Abstractions.Repositories;
using ViaTrade.Application.Common.Exceptions;
using ViaTrade.Application.Instruments.Common;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Instruments.GetBySymbol;

public sealed class GetInstrumentBySymbolHandler(IReadRepository<Instrument> instrumentRepository)
	: IQueryHandler<GetInstrumentBySymbolQuery, InstrumentResult>
{
	public async Task<InstrumentResult> HandleAsync(GetInstrumentBySymbolQuery query, CancellationToken ct = default)
	{
		return await instrumentRepository.FirstOrDefaultAsync(
				instrument => instrument.Symbol == query.Symbol,
				InstrumentResult.Projection,
				ct
			) ?? throw new NotFoundException("Instrument not found.", "instrument_not_found");
	}
}
