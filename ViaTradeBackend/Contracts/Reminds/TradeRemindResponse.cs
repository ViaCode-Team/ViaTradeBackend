namespace ViaTradeBackend.Contracts.Reminds;

public record TradeRemindResponse(
	int Id,
	string TextRemind,
	DateTime DateTime,
	int TradeCodeId,
	int UserId
);

