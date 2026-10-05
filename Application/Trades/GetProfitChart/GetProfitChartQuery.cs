using ViaTrade.Application.Common.Abstractions;

namespace ViaTrade.Application.Trades.GetProfitChart;

public sealed record GetProfitChartQuery(int UserId, ProfitChartFilter ProfitChartFilter)
	: IQuery<List<ProfitChartBucketResult>>;
