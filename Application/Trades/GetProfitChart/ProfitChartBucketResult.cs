namespace ViaTrade.Application.Trades.GetProfitChart;

public record ProfitChartBucketResult(DateOnly Date, double NetIncome, double BuyNetIncome, double SellNetIncome);
