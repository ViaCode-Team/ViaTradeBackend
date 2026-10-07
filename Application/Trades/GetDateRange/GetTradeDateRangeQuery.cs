using Mediator;

namespace ViaTrade.Application.Trades.GetDateRange;

public sealed record GetTradeDateRangeQuery() : IQuery<TradeDateRangeResult>;
