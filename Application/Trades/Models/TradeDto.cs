using Domain.Trades.Enums;

namespace Application.Trades.Models;

public record TradeDto(
	int Id,
	DateTime DateOpen,
	DateTime? DateClose,
	double TradeOpen,
	double? TradeClose,
	double? NetIncome,
	int Count,
	decimal Price,
	TradeSignal TradeSignal,
	int TradeTypeId,
	InstrumentSummaryDto? Instrument,
	int UserId
);

public record InstrumentSummaryDto(int Id, string Symbol, string? Name);

public record TradeProjectionDto(
	int Id,
	DateTime DateOpen,
	DateTime? DateClose,
	double TradeOpen,
	double? TradeClose,
	int Count,
	decimal Price,
	TradeSignal TradeSignal,
	int TradeTypeId,
	InstrumentSummaryDto? Instrument,
	int UserId
);
