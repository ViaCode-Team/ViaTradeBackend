namespace ViaTradeBackend.Contracts.Trades;

public record TradeCodeFileResponse(
	int Id,
	string ExchangeId,
	string TimeFrame,
	DateTime StartDate,
	DateTime EndDate
);

