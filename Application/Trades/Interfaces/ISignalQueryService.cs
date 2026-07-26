using Application.Common.Models;
using Application.Trades.Models;

namespace Application.Interfaces;

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

	Task<PageResult<SignalDto>> GetLatestPageAsync(int userId, PageOptions pageOptions, CancellationToken ct);
}
