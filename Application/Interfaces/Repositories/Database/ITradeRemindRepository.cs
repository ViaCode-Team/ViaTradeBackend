using Domain.Entities.DataBase;
using Domain.Models.Dto.NoteRemind;

namespace Application.Interfaces.Repositories.Database
{
    public interface ITradeRemindRepository : IRepository<TradeRemind, TradeRemindDto>
    {
        Task<IEnumerable<TradeRemind>> GetActualTradeRemind(CancellationToken cancellationToken);
        Task<IEnumerable<TradeRemind>> GetByUserAsync(int userId, CancellationToken cancellationToken);
        Task<IEnumerable<TradeRemind>> GetByUserAndTradeCodeAsync(int userId, int tradeCodeId, CancellationToken cancellationToken);
        Task<int> CountByUserAsync(int userId, CancellationToken cancellationToken);
    }
}
