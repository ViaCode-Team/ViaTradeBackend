using Domain.Entities.DataBase;
using Domain.Models.Dto.Statistic;
using ViaTradeBackend.Models.Trade;

namespace Application.Interfaces
{
    public interface ITradeService
    {
        Task<GlobalStatistic> GetGlobalStatisticAsync(int userId, CancellationToken cancellationToken);
        Task<IEnumerable<Trade>> GetByUserAsync(int userId, DateTime? startDate, DateTime? endDate, TradeSignal? tradeSignal, CancellationToken cancellationToken);
        Task<Trade> GetTradeByIdAsync(int id, int userId, CancellationToken cancellationToken);
        Task<Trade> CreateTradeAsync(TradeRequest request, int userId, CancellationToken cancellationToken);
        Task<Trade> UpdateTradeAsync(int id, TradeRequest request, int userId, CancellationToken cancellationToken);
        Task DeleteTradeAsync(int id, int userId, CancellationToken cancellationToken);
    }

}
