using Application.Common.Queries;
using Application.Common.Specifications;
using Application.Statistics.Models;
using Application.Trades.Interfaces;
using Application.Trades.Queries;
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

	public async Task<PageResult<Trade>> GetAsync(int userId, TradeFilter filter, PageOptions page, CancellationToken ct)
	{
		var spec = new TradeQuerySpecification(userId, filter);
		return await tradeRepository.GetPagedFilteredAsync(spec, page, ct);
	}
}
