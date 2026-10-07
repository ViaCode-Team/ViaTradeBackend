using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Common.Abstractions.Repositories;
using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Signals.Common;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Signals.GetLatestPage;

public sealed class GetLatestSignalsPageHandler(
	IUserContext userContext,
	SignalReader signalReader,
	IReadRepository<UserStrategyInstrument> userStrategyInstrumentRepository
) : IQueryHandler<GetLatestSignalsPageQuery, PageResult<SignalResult>>
{
	public async Task<PageResult<SignalResult>> HandleAsync(GetLatestSignalsPageQuery query, CancellationToken ct)
	{
		var sourcesSpecification = new SignalSourcesSpecification(userContext.UserId);
		var sources = await userStrategyInstrumentRepository.ListAsync(sourcesSpecification, ct);
		var signals = signalReader.ListLatestSignals(sources);

		signals = SignalReader.ApplySignalFilter(signals, query.LatestSignalFilter.Signals);

		signals = SignalReader.ApplySorting(signals, query.SignalSort.GetEffectiveSortBy()).ToList();

		return PageResult<SignalResult>.FromList(signals, query.PageOptions.Page, query.PageOptions.PageSize);
	}
}
