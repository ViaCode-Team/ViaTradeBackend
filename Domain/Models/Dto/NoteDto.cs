namespace Domain.Models.Dto
{
    public record NoteDto(int UserId, string NoteText, int TypeId, int? TradeCodeId, int? TradeStrategyId);
}
