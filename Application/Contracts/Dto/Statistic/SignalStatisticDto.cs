namespace Application.Contracts.Dto.Statistic;

public class SignalStatisticDto
{
	public required int TotalSignals { get; set; }

	public required int BuySignals { get; set; }

	public required int SellSignals { get; set; }
}
