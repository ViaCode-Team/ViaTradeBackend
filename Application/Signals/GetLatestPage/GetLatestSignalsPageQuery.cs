using Mediator;
using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Signals.Common;

namespace ViaTrade.Application.Signals.GetLatestPage;

public sealed record GetLatestSignalsPageQuery(
	LatestSignalFilter LatestSignalFilter,
	SignalSort SignalSort,
	PageOptions PageOptions
) : IQuery<PageResult<SignalResult>>;
