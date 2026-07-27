using System.ComponentModel.DataAnnotations;

namespace ViaTradeBackend.Contracts.Trades;

public record TradeTypeResponse([Range(1, int.MaxValue)] int Id, [StringLength(255)] string Name);
