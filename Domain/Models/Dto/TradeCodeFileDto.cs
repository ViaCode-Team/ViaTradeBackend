namespace Domain.Models.Dto
{
    public record TradeCodeFileDto(int Id, string ExchangeId, string TimeFrame, DateTime StartDate, DateTime EndDate);
}
