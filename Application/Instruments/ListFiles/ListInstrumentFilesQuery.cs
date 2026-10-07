using Mediator;
using ViaTrade.Application.Instruments.Common;
using ViaTrade.Domain.Enums;

namespace ViaTrade.Application.Instruments.ListFiles;

public sealed record ListInstrumentFilesQuery(TradeDataType DataType) : IQuery<IReadOnlyList<InstrumentFileResult>>;
