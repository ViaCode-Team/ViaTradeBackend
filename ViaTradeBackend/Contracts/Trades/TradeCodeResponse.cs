namespace ViaTradeBackend.Contracts.Trades;

public record TradeCodeResponse(
	int Id,
	string ExchangeId,
	string? Description
);

