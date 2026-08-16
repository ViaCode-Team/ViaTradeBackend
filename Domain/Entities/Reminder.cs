namespace ViaTrade.Domain.Entities;

public sealed class Reminder : BaseEntity<int>
{
	public required string Text { get; set; }
	public required DateTime RemindAt { get; set; }
	public required int InstrumentId { get; set; }
	public required int UserId { get; set; }
	public DateTime? PublishedAt { get; set; }
	public DateTime? DeliveredAt { get; set; }
	public Instrument? Instrument { get; set; }
	public User? User { get; set; }
}
