using Domain.Models.Dto;

namespace Application.Interfaces.Database
{
    public interface ITradeStrategyRepository
    {
        Task<TradeStrategyDto?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    }
}
