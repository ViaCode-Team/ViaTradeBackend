using Application.Intarfaces;
using Domain.Entities.DataBase;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositoryes.DataBase
{
    public interface ITradeCodeRepository : IRepository<TradeCode>
    {
        Task<TradeCode?> GetByExchangeIdAsync(string code);
    }

    public class TradeCodeRepository : GenericRepository<TradeCode>, ITradeCodeRepository
    {
        public TradeCodeRepository(AppDbContext context) : base(context) { }

        public async Task<TradeCode?> GetByExchangeIdAsync(string code)
        {
            return await _dbSet.FirstOrDefaultAsync(tc => tc.ExchangeId == code);
        }
    }
}
