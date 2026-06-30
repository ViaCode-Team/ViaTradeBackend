using Domain.Entities.DataBase;
using Domain.Models.Dto;

namespace Application.Interfaces
{
    public interface ITradeRemindService
    {
        Task<IEnumerable<TradeRemind>> GetActualRemindAsync(CancellationToken cancellationToken);
        Task DeleteActualRemindAsync(int remindId, CancellationToken cancellationToken);
        Task<IEnumerable<TradeRemind>> GetAllByUserAsync(int userId, CancellationToken cancellationToken);
        Task<IEnumerable<TradeRemind>> GetByUserAndTradeCodeAsync(int userId, int tradeCodeId, CancellationToken cancellationToken);
        Task<TradeRemind> GetByIdAsync(int remindId, int userId, CancellationToken cancellationToken);
        Task CreateAsync(int userId, int tradeCodeId, TradeRemindRequest request, CancellationToken cancellationToken);
        Task UpdateAsync(int remindId, int userId, TradeRemindRequest request, CancellationToken cancellationToken);
        Task DeleteAsync(int remindId, int userId, CancellationToken cancellationToken);
    }
}
