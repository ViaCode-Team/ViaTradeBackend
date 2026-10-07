using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Trades.Common.Abstractions;

namespace ViaTrade.Application.Trades.GetProfitChart;

public sealed class GetProfitChartHandler(IUserContext userContext, ITradeRepository tradeStatistics)
	: IQueryHandler<GetProfitChartQuery, List<ProfitChartBucketResult>>
{
	public async Task<List<ProfitChartBucketResult>> HandleAsync(GetProfitChartQuery query, CancellationToken ct)
	{
		var rows = await tradeStatistics.GetProfitChartAsync(userContext.UserId, query.ProfitChartFilter, ct);

		return rows.Select(row => new ProfitChartBucketResult(
				GetBucketDate(row, query.ProfitChartFilter.Granularity),
				row.NetIncome,
				row.BuyNetIncome,
				row.SellNetIncome
			))
			.ToList();
	}

	private static DateOnly GetBucketDate(ProfitChartAggregateRow row, ProfitChartGranularity granularity)
	{
		if (granularity == ProfitChartGranularity.Day)
			return new DateOnly(row.Year!.Value, row.Month!.Value, row.Day!.Value);

		if (granularity == ProfitChartGranularity.Month)
			return new DateOnly(row.Year!.Value, row.Month!.Value, 1);

		return DateOnly.FromDateTime(new DateTime(1900, 1, 1).AddDays(row.WeekIndex!.Value * 7));
	}
}
