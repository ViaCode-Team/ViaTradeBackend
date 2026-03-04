using Domain.Entities.DataBase;

namespace Application.Interfaces.Database
{
    public interface ITradeCodeRepository : IRepository<TradeCode>
    {
        Task<TradeCode?> GetByExchangeIdAsync(string code, CancellationToken cancellationToken = default);
    }
}
