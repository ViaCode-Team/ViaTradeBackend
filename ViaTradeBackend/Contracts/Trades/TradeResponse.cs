using System.ComponentModel.DataAnnotations;
using Domain.Enums;
using ViaTradeBackend.Contracts.Instruments;

namespace ViaTradeBackend.Contracts.Trades;

public record TradeResponse(
	[Range(1, int.MaxValue)] int Id,
	DateTime OpenedAt,
	DateTime? ClosedAt,
	[Range(double.Epsilon, double.MaxValue)] double EntryPrice,
	[Range(double.Epsilon, double.MaxValue)] double? ExitPrice,
	double? NetIncome,
	[Range(1, int.MaxValue)] int Quantity,
	[Range(typeof(decimal), "0.0000000000000000000000000001", "79228162514264337593543950335")] decimal TotalPrice,
	[EnumDataType(typeof(TradeSignal))] TradeSignal Signal,
	[Range(1, int.MaxValue)] int TradeTypeId,
	InstrumentBriefResponse? Instrument,
	[Range(1, int.MaxValue)] int UserId
);
