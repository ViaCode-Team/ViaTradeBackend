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
                .Where(tradeStrategy => tradeStrategy.Name == name)
                .Select(tradeStrategy => new TradeStrategyDto {
                    Id = tradeStrategy.Id,
                    Name = tradeStrategy.Name,
                    Description = tradeStrategy.Description,
                    Accuracy = tradeStrategy.Accuracy,
                    SignalFrequency = tradeStrategy.SignalFrequency,
                    InvestmentHorizon = tradeStrategy.InvestmentHorizon,
                    LogicDesc = tradeStrategy.LogicDesc,
                    UseDesc = tradeStrategy.UseDesc,
                    LimitDesc = tradeStrategy.LimitDesc})
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
