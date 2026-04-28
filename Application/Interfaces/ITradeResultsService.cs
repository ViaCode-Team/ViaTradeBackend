using Domain.Models.TradeLogic;

namespace Application.Interfaces
{
    public interface ITradeResultsService
    {
        Task<StrategyResultResponse> GetStrategyResultAsync(
            int userId,
            DateTime? startDate,
            DateTime? endDate,
            CancellationToken cancellationToken);
        Task<StrategyResultResponse> GetStrategyResultByCodeAsync(
            int userId,
            string strategyName,
            string tradeCode,
            DateTime? startDate,
            DateTime? endDate,
            CancellationToken cancellationToken);
    }
}
