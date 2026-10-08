using Mediator;

namespace ViaTrade.Application.Trades.GetProfitChart;

public sealed record GetProfitChartQuery(ProfitChartFilter ProfitChartFilter) : IQuery<List<ProfitChartBucketResult>>;
