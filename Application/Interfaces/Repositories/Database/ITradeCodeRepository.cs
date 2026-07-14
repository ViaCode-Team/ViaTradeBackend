using Domain.Entities.DataBase;
using Domain.Models.Dto.Trade;

namespace Application.Interfaces.Repositories.Database;

public interface ITradeCodeRepository : IRepository<TradeCode, TradeCodeDto>
{
	Task<int> CountAsync(CancellationToken cancellationToken = default);
	Task<TradeCodeDto?> GetByExchangeIdAsync(string code, CancellationToken cancellationToken = default);
}
