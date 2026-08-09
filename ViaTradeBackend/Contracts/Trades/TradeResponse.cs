using System.ComponentModel.DataAnnotations;
using Domain.Enums;
using ViaTradeBackend.Contracts.Instruments;

namespace ViaTradeBackend.Contracts.Trades;

public record TradeResponse(
	[Range(1, int.MaxValue)] int Id,
	DateTime OpenedAt,
	DateTime? ClosedAt,
	[Range(double.Epsilon, double.MaxValue)] double OpenPrice,
	[Range(double.Epsilon, double.MaxValue)] double? ClosePrice,
	double? NetIncome,
	[Range(1, int.MaxValue)] int Quantity,
	decimal TotalPrice,
	[EnumDataType(typeof(TradeSignal))] TradeSignal Signal,
	[Range(1, int.MaxValue)] int TradeTypeId,
	InstrumentBriefResponse? Instrument,
	[Range(1, int.MaxValue)] int UserId
);
