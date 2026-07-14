using Application.Interfaces.Repositories.Database;
using Domain.Entities.DataBase;
using Domain.Models.Dto.Trade;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.DataBase
{
    public class TradeCodeRepository(AppDbContext context) : GenericRepository<TradeCode, TradeCodeDto>(context), ITradeCodeRepository
    {
        public async Task<int> CountAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet.CountAsync(cancellationToken);
        }

        public async Task<TradeCodeDto?> GetByExchangeIdAsync(string code, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(e => e.ExchangeId == code)
                .Select(e => new TradeCodeDto
                {
                    Id = e.Id,
                    ExchangeId = e.ExchangeId,
                    Description = e.Description
                })
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
