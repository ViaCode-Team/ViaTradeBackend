using System.ComponentModel.DataAnnotations;

namespace ViaTrade.Api.Contracts.Trades;

public record TradeTypeResponse([Range(1, int.MaxValue)] int Id, [StringLength(255)] string Name);
