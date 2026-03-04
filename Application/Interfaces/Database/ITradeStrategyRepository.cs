using Domain.Entities.DataBase;

namespace Application.Interfaces.Database
{
    public interface ITradeStrategyRepository : IRepository<TradeStrategy>
    {
        Task<TradeStrategy?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    }
}
