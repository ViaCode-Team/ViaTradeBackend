using Domain.Models.Dto.Strategy;

namespace Application.Interfaces.Database
{
    public interface ITradeStrategyRepository
    {
        Task<TradeStrategyDto?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    }
}
