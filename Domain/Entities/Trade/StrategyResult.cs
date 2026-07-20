namespace Domain.Trades.Entities;

public class StrategyResult
{
	public required DateTime Date { get; set; }

	public required decimal ClosePrice { get; set; }

	public required string Signal { get; set; }
}
