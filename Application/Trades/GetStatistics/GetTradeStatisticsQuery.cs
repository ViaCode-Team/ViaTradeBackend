using ViaTrade.Application.Common.Abstractions;

namespace ViaTrade.Application.Trades.GetStatistics;

public sealed record GetTradeStatisticsQuery() : IQuery<TradeStatisticsResult>;
