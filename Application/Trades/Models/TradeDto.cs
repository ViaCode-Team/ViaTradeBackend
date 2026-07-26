using Domain.Enums;

namespace Application.Trades.Models;

public record TradeDto(
	int Id,
	DateTime OpenedAt,
	DateTime? ClosedAt,
	double EntryPrice,
	double? ExitPrice,
	double? NetIncome,
	int Quantity,
	decimal TotalPrice,
	TradeSignal Signal,
	int TradeTypeId,
	InstrumentSummaryDto? Instrument,
	int UserId
);

public record InstrumentSummaryDto(int Id, string Symbol, string? Name);

public record TradeProjectionDto(
	int Id,
	DateTime OpenedAt,
	DateTime? ClosedAt,
	double EntryPrice,
	double? ExitPrice,
	int Quantity,
	decimal TotalPrice,
	TradeSignal Signal,
	int TradeTypeId,
	InstrumentSummaryDto? Instrument,
	int UserId
);
