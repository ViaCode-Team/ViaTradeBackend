using Mediator;
using ViaTrade.Application.Instruments.Common;
using ViaTrade.Domain.Enums;

namespace ViaTrade.Application.Instruments.GetFile;

public sealed record GetInstrumentFileQuery(TradeDataType DataType, string InstrumentIdOrTicker)
	: IQuery<InstrumentFileResult>;
