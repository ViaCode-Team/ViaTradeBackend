namespace Domain.Entities;

public sealed class Note : BaseEntity<int>
{
	public required int UserId { get; set; }

	public required string Text { get; set; }

	public int? InstrumentId { get; set; }

	public int? StrategyId { get; set; }

	public User? User { get; set; }

	public Instrument? Instrument { get; set; }

	public Strategy? Strategy { get; set; }
}
