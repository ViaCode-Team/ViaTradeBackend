using Application.Common.Models.Filters;
using Application.Common.Models.Pagination;
using Application.Common.Specifications;
using Application.Statistics.Models;
using Application.Trades.Interfaces;
using Domain.Trades.Entities;

namespace Application.Trades;

public class TradeQueryService(ITradeRepository tradeRepository) : ITradeQueryService
{
	public async Task<GlobalStatisticReadModel> GetStatisticsAsync(int userId, CancellationToken ct)
	{
		return await tradeRepository.GetGlobalStatisticAsync(userId, ct);
	}

	public async Task<Trade> GetAsync(int id, int userId, CancellationToken ct)
	{
		var trade = await tradeRepository.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, ct);
		if (trade == null)
			throw new KeyNotFoundException();

		return trade;
	}

	public async Task<PagedResult<Trade>> GetAsync(int userId, TradeFilterRequest? filterRequest, PaginationRequest? paginationRequest, CancellationToken ct)
	{
		var spec = new TradeQuerySpecification(userId, filterRequest);
		return await tradeRepository.GetPagedFilteredAsync(spec, paginationRequest, ct);
	}
}
