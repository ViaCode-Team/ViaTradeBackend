namespace ViaTrade.Domain.Entities;

public sealed class Instrument : BaseEntity<int>
{
	public required string Symbol { get; set; }

	public string? Description { get; set; }

	public ICollection<Trade> Trades { get; set; } = [];

	public ICollection<UserInstrument> UserInstruments { get; set; } = [];
}
