using Domain.Entities.DataBase;

namespace Application.Intarfaces.Database
{
    public interface ITradeStrategyRepository : IRepository<TradeStrategy>
    {
        Task<TradeStrategy?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    }
}
