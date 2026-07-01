using Domain.Entities.DataBase;
using Domain.Models.Dto.Strategy;

namespace Application.Interfaces.Repositories.Database
{
    public interface ITradeStrategyRepository : IRepository<TradeStrategy, TradeStrategyDto>
    {
        Task<int> CountAsync(CancellationToken cancellationToken = default);
        Task<TradeStrategyDto?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    }
}
