using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Trades.Models;

namespace ViaTrade.Application.Trades.Interfaces;

public interface ISignalQueryService
{
	Task<SignalStatisticDto> GetStatisticsAsync(int userId, CancellationToken ct);

	Task<PageResult<SignalDto>> GetPageAsync(
		int userId,
		SignalHistoryFilter signalHistoryFilter,
		SignalSort signalSort,
		PageOptions pageOptions,
		CancellationToken ct
	);

	Task<PageResult<SignalDto>> GetLatestPageAsync(
		int userId,
		LatestSignalFilter latestSignalFilter,
		SignalSort signalSort,
		PageOptions pageOptions,
		CancellationToken ct
	);
}
