namespace Application.Statistics.Models;

public class SignalStatisticReadModel
{
	public required int TotalSignals { get; set; }

	public required int BuySignals { get; set; }

	public required int SellSignals { get; set; }
}

