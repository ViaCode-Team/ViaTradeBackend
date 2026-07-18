using System.ComponentModel.DataAnnotations;

namespace Application.Reminds.Models;

public record TradeRemindCreateDto
{
	[StringLength(1024)]
	public required string TextRemind { get; set; }

	public required DateTime DateTime { get; set; }
}
