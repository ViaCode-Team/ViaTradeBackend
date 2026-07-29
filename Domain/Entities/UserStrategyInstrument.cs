namespace Domain.Entities;

public sealed class UserStrategyInstrument : BaseEntity<int>
{
	public required int UserId { get; set; }
	public required int InstrumentId { get; set; }
	public required int StrategyId { get; set; }

	public User? User { get; set; }

	public Instrument? Instrument { get; set; }

	public Strategy? Strategy { get; set; }
}
