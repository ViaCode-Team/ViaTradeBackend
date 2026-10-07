using Mediator;
using ViaTrade.Application.Common.Abstractions.Repositories;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Instruments.GetStatistics;

public sealed class GetInstrumentStatisticsHandler(IReadRepository<Instrument> instrumentRepository)
	: IQueryHandler<GetInstrumentStatisticsQuery, InstrumentStatisticsResult>
{
	public async ValueTask<InstrumentStatisticsResult> Handle(GetInstrumentStatisticsQuery query, CancellationToken ct)
	{
		int totalInstruments = await instrumentRepository.CountAsync(ct);

		return new InstrumentStatisticsResult(totalInstruments);
	}
}
