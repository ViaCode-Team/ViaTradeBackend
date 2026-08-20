using Microsoft.EntityFrameworkCore;
using ViaTrade.Application.Common.Interfaces;
using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Trades.Interfaces;
using ViaTrade.Application.Trades.Models;
using ViaTrade.Domain.Entities;
using ViaTrade.Infrastructure.DataBase.Extensions;

namespace ViaTrade.Infrastructure.DataBase.Repositories;

public class TradeEfRepository(AppDbContext context) : BaseEfRepository<Trade>(context), ITradeRepository
{
	private static readonly DateTime WeekEpoch = new(1900, 1, 1);

	public async Task<List<ProfitChartAggregateRow>> GetProfitChartAsync(
		int userId,
		ProfitChartFilter profitChartFilter,
		CancellationToken ct
	)
	{
		var tradesQuery = GetClosedTradesQuery(userId, profitChartFilter.StartDate, profitChartFilter.EndDate);

		return profitChartFilter.Granularity switch
		{
			ProfitChartGranularity.Day => await tradesQuery
				.GroupBy(trade => new
				{
					trade.ClosedAt!.Value.Year,
					trade.ClosedAt.Value.Month,
					trade.ClosedAt.Value.Day,
				})
				.OrderBy(group => group.Key.Year)
				.ThenBy(group => group.Key.Month)
				.ThenBy(group => group.Key.Day)
				.Select(group => group.ToProfitChartAggregateRow(group.Key.Year, group.Key.Month, group.Key.Day, null))
				.ToListAsync(ct),

			ProfitChartGranularity.Week => await tradesQuery
				.GroupBy(trade => EF.Functions.DateDiffWeek(WeekEpoch, trade.ClosedAt!.Value))
				.OrderBy(group => group.Key)
				.Select(group => group.ToProfitChartAggregateRow(null, null, null, group.Key))
				.ToListAsync(ct),

			ProfitChartGranularity.Month => await tradesQuery
				.GroupBy(trade => new { trade.ClosedAt!.Value.Year, trade.ClosedAt.Value.Month })
				.OrderBy(group => group.Key.Year)
				.ThenBy(group => group.Key.Month)
				.Select(group => group.ToProfitChartAggregateRow(group.Key.Year, group.Key.Month, null, null))
				.ToListAsync(ct),

			_ => throw new ArgumentOutOfRangeException(nameof(profitChartFilter.Granularity)),
		};
	}

	public async Task<TradeDateRangeDto> GetTradeDateRangeAsync(int userId, CancellationToken ct)
	{
		var range = await GetClosedTradesQuery(userId, null, null)
			.GroupBy(_ => 1)
			.Select(group => new
			{
				MinDate = group.Min(trade => trade.ClosedAt!.Value),
				MaxDate = group.Max(trade => trade.ClosedAt!.Value),
			})
			.SingleOrDefaultAsync(ct);

		if (range == null)
			return new TradeDateRangeDto(null, null);

		return new TradeDateRangeDto(DateOnly.FromDateTime(range.MinDate), DateOnly.FromDateTime(range.MaxDate));
	}

	public async Task<TradeProjectionDto?> FindProjectionByUserAndIdAsync(int userId, int id, CancellationToken ct)
	{
		return await _dbSet
			.Where(trade => trade.Id == id && trade.UserId == userId)
			.Select(trade => trade.ToTradeProjectionDto())
			.FirstOrDefaultAsync(ct);
	}

	public async Task<PageResult<TradeProjectionDto>> GetPageProjectionAsync(
		IQueryObject<Trade> queryObject,
		PageOptions pageOptions,
		CancellationToken ct
	)
	{
		var query = QueryObjectEvaluator.GetQueryForPagination(_dbSet, queryObject, _entityType);

		return await query.Select(trade => trade.ToTradeProjectionDto()).ToPagedAsync(pageOptions, ct);
	}

	public async Task<TradeStatisticAggregateDto> GetGlobalStatisticsAsync(int userId, CancellationToken ct)
	{
		var result = await _context
			.Trades.Where(trade => trade.UserId == userId && trade.IsClosedTrade() && trade.IsProfitCalculable())
			.Select(trade => new { Income = trade.NetIncome!.Value })
			.GroupBy(_ => 1)
			.Select(group => new TradeStatisticAggregateDto(
				group.Count(),
				group.Count(trade => trade.Income > 0),
				group.Count(trade => trade.Income < 0),
				group.Sum(trade => Math.Abs(trade.Income)),
				group.Where(trade => trade.Income > 0).Sum(trade => trade.Income),
				group.Where(trade => trade.Income < 0).Sum(trade => -trade.Income)
			))
			.SingleOrDefaultAsync(ct);

		return result ?? TradeStatisticAggregateDto.Empty;
	}

	public async Task<int> ExecuteUpdateAsync(
		int userId,
		int id,
		TradeInputDto request,
		decimal price,
		CancellationToken ct
	)
	{
		return await EfDatabaseOperation.ExecuteAsync(() =>
			_dbSet
				.Where(t => t.Id == id && t.UserId == userId)
				.ExecuteUpdateAsync(
					s =>
						s.SetProperty(t => t.OpenedAt, request.OpenedAt)
							.SetProperty(t => t.ClosedAt, request.ClosedAt)
							.SetProperty(t => t.OpenPrice, request.OpenPrice)
							.SetProperty(t => t.ClosePrice, request.ClosePrice)
							.SetProperty(t => t.Quantity, request.Quantity)
							.SetProperty(t => t.Signal, request.Signal)
							.SetProperty(t => t.TotalPrice, price)
							.SetProperty(t => t.TradeTypeId, request.TradeTypeId)
							.SetProperty(t => t.InstrumentId, request.InstrumentId),
					ct
				)
		);
	}

	private IQueryable<Trade> GetClosedTradesQuery(int userId, DateOnly? startDate, DateOnly? endDate)
	{
		var query = _context.Trades.Where(trade =>
			trade.UserId == userId && trade.IsClosedTrade() && trade.IsProfitCalculable()
		);

		if (startDate.HasValue)
			query = query.Where(trade => trade.ClosedAt!.Value >= startDate.Value.ToDateTime(TimeOnly.MinValue));

		if (endDate.HasValue)
			query = query.Where(trade =>
				trade.ClosedAt!.Value < endDate.Value.AddDays(1).ToDateTime(TimeOnly.MinValue)
			);

		return query;
	}
}
