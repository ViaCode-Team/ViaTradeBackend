using Domain.Models.Dto.Statistic;
using Domain.Models.TradeLogic;

namespace Application.Interfaces
{
    public interface ITradeResultsService
    {
        Task<SignalStatistic> GetStrategyResultStatisticAsync(
            int userId, 
            CancellationToken cancellationToken);

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
