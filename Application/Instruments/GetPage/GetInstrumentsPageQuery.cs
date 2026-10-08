using Mediator;
using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Instruments.Common;

namespace ViaTrade.Application.Instruments.GetPage;

public sealed record GetInstrumentsPageQuery(
	InstrumentFilter InstrumentFilter,
	InstrumentSearch InstrumentSearch,
	PageOptions PageOptions,
	InstrumentSort InstrumentSort
) : IQuery<PageResult<InstrumentResult>>;
