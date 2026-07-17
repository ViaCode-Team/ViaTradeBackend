namespace Domain.Models.Dto.NoteRemind;

public class TradeRemindDto
{
	public required int Id { get; set; }

	public required string TextRemind { get; set; }

	public required DateTime DateTime { get; set; }

	public required int TradeCodeId { get; set; }

	public required int UserId { get; set; }
}
