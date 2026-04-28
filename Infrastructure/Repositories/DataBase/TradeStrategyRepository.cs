using Application.Interfaces.Database;
using Domain.Entities.DataBase;
using Domain.Models.Dto;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositoryes.DataBase
{
    public class TradeStrategyRepository(AppDbContext context) : GenericRepository<TradeStrategy, TradeStrategyDto>(context),
        ITradeStrategyRepository
    {
        public async Task<TradeStrategyDto?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(ts => ts.Name == name)
                .Select(ts => new TradeStrategyDto(ts.Name, ts.Description, ts.Accuracy))
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
