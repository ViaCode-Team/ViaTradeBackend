using Domain.Entities.DataBase;
using Domain.Models.Dto.NoteRemind;

namespace Application.Interfaces.Database
{
    public interface ITradeRemindRepository : IRepository<TradeRemind, TradeRemindDto>
    {
        Task<IEnumerable<TradeRemind>> GetByUserAsync(int userId, CancellationToken cancellationToken);
        Task<IEnumerable<TradeRemind>> GetByUserAndTradeCodeAsync(int userId, int tradeCodeId, CancellationToken cancellationToken);
    }
}