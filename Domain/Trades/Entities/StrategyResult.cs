namespace Domain.Trades.Entities;

public class StrategyResult
{
	public DateTime Date { get; set; }

	public decimal ClosePrice { get; set; }

	public required string Signal { get; set; }
}
