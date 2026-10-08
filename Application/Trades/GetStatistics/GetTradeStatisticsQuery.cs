using Mediator;

namespace ViaTrade.Application.Trades.GetStatistics;

public sealed record GetTradeStatisticsQuery() : IQuery<TradeStatisticsResult>;
