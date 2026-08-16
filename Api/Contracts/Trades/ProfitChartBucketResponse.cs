namespace ViaTrade.Api.Contracts.Trades;

public record ProfitChartBucketResponse(DateOnly Date, double NetIncome, double BuyNetIncome, double SellNetIncome);
