using Domain.Entities.DataBase;
using Domain.Models.Dto.Trade;

namespace Application.Interfaces.Database
{
    public interface ITradeCodeRepository
    {
        Task<TradeCodeDto?> GetByExchangeIdAsync(string code, CancellationToken cancellationToken = default);
    }
}
