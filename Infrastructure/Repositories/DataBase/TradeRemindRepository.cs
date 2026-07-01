using Application.Interfaces.Database;
using Domain.Entities.DataBase;
using Domain.Models.Dto.NoteRemind;
using Infrastructure.Repositories.DataBase;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.DataBase
{
    public class TradeRemindRepository(AppDbContext context)
        : GenericRepository<TradeRemind, TradeRemindDto>(context), ITradeRemindRepository
    {
        public async Task<IEnumerable<TradeRemind>> GetActualTradeRemind(CancellationToken cancellationToken)
        {
            return await _dbSet.Where(r => r.DateTime <= DateTime.Now).ToListAsync(cancellationToken);
        }
        
        public async Task<IEnumerable<TradeRemind>> GetByUserAsync(int userId, CancellationToken cancellationToken)
        {
            return await _dbSet.Where(r => r.UserId == userId).ToListAsync(cancellationToken);
        }

        public async Task<int> CountByUserAsync(int userId, CancellationToken cancellationToken)
        {
            return await _dbSet.CountAsync(r => r.UserId == userId, cancellationToken);
        }

        public async Task<IEnumerable<TradeRemind>> GetByUserAndTradeCodeAsync(int userId, int tradeCodeId, CancellationToken cancellationToken)
        {
            return await _dbSet.Where(r => r.UserId == userId && r.TradeCodeId == tradeCodeId).ToListAsync(cancellationToken);
        }
    }
}
