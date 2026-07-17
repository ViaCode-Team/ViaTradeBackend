namespace Domain.Models.Dto.Statistic;

public class SignalStatistic
{
	public required int TotalSignals { get; set; }

	public required int BuySignals { get; set; }

	public required int SellSignals { get; set; }
}
