using Application.Interfaces.Database;
using Domain.Entities.DataBase;
using Domain.Models.Dto;
using Infrastructure.Repositoryes.DataBase;
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
                .Select(e => new UserStrategyTradeCodeDto(e.UserId, e.TradeCodeId, e.StrategyId))
                .ToListAsync(cancellationToken);
        }
    }
}
