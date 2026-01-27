using Application.Intarfaces;
using Domain.Entities.DataBase;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositoryes.DataBase
{
    public interface ITradeStrategyRepository : IRepository<TradeStrategy>
    {
        Task<TradeStrategy?> GetByNameAsync(string name);
    }

    public class TradeStrategyRepository : GenericRepository<TradeStrategy>, ITradeStrategyRepository
    {
        public TradeStrategyRepository(AppDbContext context) : base(context) { }

        public async Task<TradeStrategy?> GetByNameAsync(string name)
        {
            return await _dbSet.FirstOrDefaultAsync(ts => ts.Name == name);
        }
    }
}
