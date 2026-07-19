using System.ComponentModel.DataAnnotations;

namespace ViaTradeBackend.Contracts.Reminds;

public record CreateTradeRemindRequest(
	[StringLength(1024)] string TextRemind,
	DateTime DateTime
);

