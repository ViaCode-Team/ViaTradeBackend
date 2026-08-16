namespace ViaTrade.Api.Contracts.Trades;

public record TradeDateRangeResponse(DateOnly? MinDate, DateOnly? MaxDate);
