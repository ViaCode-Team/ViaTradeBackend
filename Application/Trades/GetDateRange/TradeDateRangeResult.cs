namespace ViaTrade.Application.Trades.GetDateRange;

public sealed record TradeDateRangeResult(DateOnly? MinDate, DateOnly? MaxDate);
