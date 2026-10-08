namespace ViaTrade.Application.Trades.GetProfitChart;

public record ProfitChartAggregateRow(
	int? Year,
	int? Month,
	int? Day,
	double? WeekIndex,
	double NetIncome,
	double BuyNetIncome,
	double SellNetIncome
);
