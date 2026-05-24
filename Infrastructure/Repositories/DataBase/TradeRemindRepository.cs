using Application.Interfaces.Database;
using Domain.Entities.DataBase;
using Domain.Models.Dto.NoteRemind;
using Infrastructure.Repositoryes.DataBase;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.DataBase
{
    public class TradeRemindRepository(AppDbContext context)
        : GenericRepository<TradeRemind, TradeRemindDto>(context), ITradeRemindRepository
    {
        public async Task<IEnumerable<TradeRemind>> GetByUserAsync(int userId, CancellationToken cancellationToken)
        {
            return await _dbSet.Where(r => r.UserId == userId).ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<TradeRemind>> GetByUserAndTradeCodeAsync(int userId, int tradeCodeId, CancellationToken cancellationToken)
        {
            return await _dbSet.Where(r => r.UserId == userId && r.TradeCodeId == tradeCodeId).ToListAsync(cancellationToken);
        }
    }
}