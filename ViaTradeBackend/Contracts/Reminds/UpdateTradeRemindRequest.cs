using System.ComponentModel.DataAnnotations;

namespace ViaTradeBackend.Contracts.Reminds;

public record UpdateTradeRemindRequest(
	[StringLength(1024)] string TextRemind,
	DateTime DateTime
);

