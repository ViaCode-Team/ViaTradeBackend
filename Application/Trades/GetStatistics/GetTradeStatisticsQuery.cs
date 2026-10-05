using ViaTrade.Application.Common.Abstractions;

namespace ViaTrade.Application.Trades.GetStatistics;

public sealed record GetTradeStatisticsQuery(int UserId) : IQuery<TradeStatisticsResult>;
