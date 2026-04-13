using Domain.Entities.DataBase;
using Domain.Models.Dto;

namespace Application.Interfaces.Database
{
    public interface ITradeCodeRepository
    {
        Task<TradeCodeDto?> GetByExchangeIdAsync(string code, CancellationToken cancellationToken = default);
    }
}
