using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Instruments.Common;

namespace ViaTrade.Application.Strategies.GetInstrumentsPage;

public sealed record GetStrategyInstrumentsPageQuery(
	int StrategyId,
	StrategyInstrumentFilter InstrumentFilter,
	InstrumentSort InstrumentSort,
	PageOptions PageOptions
) : IQuery<PageResult<InstrumentResult>>;
