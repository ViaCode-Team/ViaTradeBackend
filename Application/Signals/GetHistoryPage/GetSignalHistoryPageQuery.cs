using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Signals.Common;

namespace ViaTrade.Application.Signals.GetHistoryPage;

public sealed record GetSignalHistoryPageQuery(
	SignalHistoryFilter SignalHistoryFilter,
	SignalSort SignalSort,
	PageOptions PageOptions
) : IQuery<PageResult<SignalResult>>;
