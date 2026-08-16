namespace ViaTrade.Application.Instruments.Models;

public record InstrumentLinkStatusDto(int Id, string Symbol, string? Description, bool IsLinkedToStrategy);
