namespace ViaTradeBackend.Contracts.Trades;

public record TradeDateRangeResponse(DateOnly? MinDate, DateOnly? MaxDate);
