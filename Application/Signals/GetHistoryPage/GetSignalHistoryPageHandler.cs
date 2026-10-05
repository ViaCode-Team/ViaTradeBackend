using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Common.Abstractions.Repositories;
using ViaTrade.Application.Common.Exceptions;
using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Signals.Common;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Signals.GetHistoryPage;

public sealed class GetSignalHistoryPageHandler(
	SignalReader signalReader,
	IReadRepository<UserStrategyInstrument> userStrategyInstrumentRepository
) : IQueryHandler<GetSignalHistoryPageQuery, PageResult<SignalResult>>
{
	public async Task<PageResult<SignalResult>> HandleAsync(
		GetSignalHistoryPageQuery query,
		CancellationToken ct = default
	)
	{
		var sourcesSpecification = new SignalSourcesSpecification(query.UserId);
		var sources = await userStrategyInstrumentRepository.ListAsync(sourcesSpecification, ct);
		sources = sources
			.Where(source => source.StrategyId == query.SignalHistoryFilter.StrategyId)
			.Where(source => source.InstrumentId == query.SignalHistoryFilter.InstrumentId)
			.ToList();
		if (sources.Count == 0)
			throw new NotFoundException("Strategy instrument link was not found.", "strategy_instrument_not_found");

		var signals = signalReader.ListSignals(
			sources,
			query.SignalHistoryFilter.StartDate,
			query.SignalHistoryFilter.EndDate,
			query.SignalSort
		);

		signals = SignalReader.ApplySignalFilter(signals, query.SignalHistoryFilter.Signals);

		return PageResult<SignalResult>.FromList(signals, query.PageOptions.Page, query.PageOptions.PageSize);
	}
}
