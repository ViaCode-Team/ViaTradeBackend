using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Common.Abstractions.Repositories;
using ViaTrade.Application.Common.Exceptions;
using ViaTrade.Application.Instruments.Common;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Instruments.GetByTicker;

public sealed class GetInstrumentByTickerHandler(IReadRepository<Instrument> instrumentRepository)
	: IQueryHandler<GetInstrumentByTickerQuery, InstrumentResult>
{
	public async Task<InstrumentResult> HandleAsync(GetInstrumentByTickerQuery query, CancellationToken ct)
	{
		return await instrumentRepository.FirstOrDefaultAsync(
				instrument => instrument.Ticker == query.Ticker,
				InstrumentResult.Projection,
				ct
			) ?? throw new NotFoundException("Instrument not found.", "instrument_not_found");
	}
}
