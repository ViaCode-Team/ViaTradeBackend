using Domain.Entities.DataBase;
using Domain.Models.Dto.Strategy;
using ViaTradeBackend.Models.Trade;

namespace Application.Interfaces
{
    public interface IStrategyService
    {
        Task<IEnumerable<TradeStrategy>> GetAllStrategiesAsync(CancellationToken cancellationToken);
        Task<TradeStrategy> GetStrategyByIdAsync(int strategyId, CancellationToken cancellationToken);
        Task<IEnumerable<UserStrategyTradeCodeDto>> GetUserStrategyCodesAsync(int userId, CancellationToken cancellationToken);
        Task CreateUserStrategyCodeAsync(UserStrategyTradeCodeRequest request, int userId, CancellationToken cancellationToken);
        Task DeleteUserStrategyCodeAsync(int strategyId, int tradeCodeId, int userId, CancellationToken cancellationToken);
        Task<IEnumerable<UserTradeStrategyDto>> GetUserStrategiesAsync(int userId, CancellationToken cancellationToken);
        Task CreateUserStrategyAsync(CreateUserStrategyRequest request, int userId, CancellationToken cancellationToken);
        Task DeleteUserStrategyAsync(int strategyId, int userId, CancellationToken cancellationToken);
    }
}
