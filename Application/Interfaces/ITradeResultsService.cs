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
    }
}
