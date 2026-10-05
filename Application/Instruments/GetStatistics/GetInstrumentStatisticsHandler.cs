using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Common.Abstractions.Repositories;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Instruments.GetStatistics;

public sealed class GetInstrumentStatisticsHandler(IReadRepository<Instrument> instrumentRepository)
	: IQueryHandler<GetInstrumentStatisticsQuery, InstrumentStatisticsResult>
{
	public async Task<InstrumentStatisticsResult> HandleAsync(
		GetInstrumentStatisticsQuery query,
		CancellationToken ct = default
	)
	{
		int totalInstruments = await instrumentRepository.CountAsync(ct);

		return new InstrumentStatisticsResult(totalInstruments);
	}
}
