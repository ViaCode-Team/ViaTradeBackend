using Domain.Entities.DataBase;
using Domain.Models.Dto.Trade;

namespace Application.Interfaces.Database
{
    public interface ITradeRepository : IRepository<Trade, TradeDto>
    {
        Task<IEnumerable<Trade>> GetByUserAsync(int userId, CancellationToken cancellationToken);
        Task<IEnumerable<Trade>> GetByUserAndTradeCodeAsync(int userId, int tradeCodeId, CancellationToken cancellationToken);
        Task<IEnumerable<Trade>> GetByUserAndDateRangeAsync(int userId, DateTime? from, DateTime? to, TradeSignal? tradeSignal, CancellationToken cancellationToken);
    }
}
