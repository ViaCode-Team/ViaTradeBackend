using Mediator;
using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Common.Abstractions.Repositories;
using ViaTrade.Application.Signals.Common;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Signals.GetStatistics;

public sealed class GetSignalStatisticsHandler(
	IUserContext userContext,
	SignalReader signalReader,
	IReadRepository<UserStrategyInstrument> userStrategyInstrumentRepository
) : IQueryHandler<GetSignalStatisticsQuery, SignalStatisticsResult>
{
	public async ValueTask<SignalStatisticsResult> Handle(GetSignalStatisticsQuery query, CancellationToken ct)
	{
		var specification = new SignalSourcesSpecification(userContext.UserId);

		var sources = await userStrategyInstrumentRepository.ListAsync(specification, ct);
		var signals = signalReader.ListSignals(sources, null, null, new SignalSort());

		return new SignalStatisticsResult(
			signals.Count,
			signals.Count(signal => signal.Signal == "BUY"),
			signals.Count(signal => signal.Signal == "SELL")
		);
	}
}
