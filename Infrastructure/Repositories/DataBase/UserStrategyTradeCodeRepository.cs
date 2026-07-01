using Application.Interfaces.Repositories.Database;
using Domain.Entities.DataBase;
using Domain.Models.Dto.Strategy;
using Infrastructure.Repositories.DataBase;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.DataBase
{
    public class UserStrategyTradeCodeRepository(AppDbContext context) : GenericRepository<UserStrategyTradeCode, UserStrategyTradeCodeDto>(context),
        IUserStrategyTradeCodeRepository
    {
        public async Task<IEnumerable<UserStrategyTradeCodeDto>> GetAllAsync(int userId, CancellationToken cancellationToken)
        {
            return await _dbSet
                .Where(e => e.UserId == userId)
                .Select(e => new UserStrategyTradeCodeDto {
                    UserId = e.UserId,
                    TradeCodeId = e.TradeCodeId,
                    StrategyId = e.StrategyId })
                .ToListAsync(cancellationToken);
        }
    }
}
