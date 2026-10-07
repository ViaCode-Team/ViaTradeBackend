using ViaTrade.Application.Common.Abstractions;

namespace ViaTrade.Application.Trades.GetDateRange;

public sealed record GetTradeDateRangeQuery() : IQuery<TradeDateRangeResult>;
