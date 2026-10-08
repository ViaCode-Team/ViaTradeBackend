using Microsoft.EntityFrameworkCore;
using ViaTrade.Application.Trades.Common.Abstractions;
using ViaTrade.Application.Trades.GetDateRange;
using ViaTrade.Application.Trades.GetProfitChart;
using ViaTrade.Application.Trades.GetStatistics;
using ViaTrade.Domain.Entities;
using ViaTrade.Domain.Enums;

namespace ViaTrade.Infrastructure.DataBase.Repositories;

public class TradeEfRepository(AppDbContext context) : ITradeRepository
{
	private static readonly DateTime WeekEpoch = new(1900, 1, 1);

	public async Task<List<ProfitChartAggregateRow>> GetProfitChartAsync(
		int userId,
		ProfitChartFilter profitChartFilter,
		CancellationToken ct
	)
	{
		var projectedQuery = GetClosedTradesQuery(userId, profitChartFilter.StartDate, profitChartFilter.EndDate)
			.Select(trade => new
			{
				trade.ClosedAt!.Value.Year,
				trade.ClosedAt.Value.Month,
				trade.ClosedAt.Value.Day,
				Week = EF.Functions.DateDiffWeek(WeekEpoch, trade.ClosedAt.Value),
				Income = trade.NetIncome!.Value,
				BuyIncome = trade.Signal == TradeSignal.BUY ? trade.NetIncome!.Value : 0,
				SellIncome = trade.Signal == TradeSignal.SELL ? trade.NetIncome!.Value : 0,
			});

		return profitChartFilter.Granularity switch
		{
			ProfitChartGranularity.Day => await projectedQuery
				.GroupBy(t => new
				{
					t.Year,
					t.Month,
					t.Day,
				})
				.OrderBy(g => g.Key.Year)
				.ThenBy(g => g.Key.Month)
				.ThenBy(g => g.Key.Day)
				.Select(g => new ProfitChartAggregateRow(
					g.Key.Year,
					g.Key.Month,
					g.Key.Day,
					null,
					Math.Round(g.Sum(x => x.Income), 2),
					Math.Round(g.Sum(x => x.BuyIncome), 2),
					Math.Round(g.Sum(x => x.SellIncome), 2)
				))
				.ToListAsync(ct),

			ProfitChartGranularity.Week => await projectedQuery
				.GroupBy(t => t.Week)
				.OrderBy(g => g.Key)
				.Select(g => new ProfitChartAggregateRow(
					null,
					null,
					null,
					g.Key,
					Math.Round(g.Sum(x => x.Income), 2),
					Math.Round(g.Sum(x => x.BuyIncome), 2),
					Math.Round(g.Sum(x => x.SellIncome), 2)
				))
				.ToListAsync(ct),

			ProfitChartGranularity.Month => await projectedQuery
				.GroupBy(t => new { t.Year, t.Month })
				.OrderBy(g => g.Key.Year)
				.ThenBy(g => g.Key.Month)
				.Select(g => new ProfitChartAggregateRow(
					g.Key.Year,
					g.Key.Month,
					null,
					null,
					Math.Round(g.Sum(x => x.Income), 2),
					Math.Round(g.Sum(x => x.BuyIncome), 2),
					Math.Round(g.Sum(x => x.SellIncome), 2)
				))
				.ToListAsync(ct),

			_ => throw new ArgumentOutOfRangeException(nameof(profitChartFilter.Granularity)),
		};
	}

	public async Task<TradeDateRangeResult> GetTradeDateRangeAsync(int userId, CancellationToken ct)
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
			return new TradeDateRangeResult(null, null);

		return new TradeDateRangeResult(DateOnly.FromDateTime(range.MinDate), DateOnly.FromDateTime(range.MaxDate));
	}

	public async Task<TradeStatisticsAggregate> GetGlobalStatisticsAsync(int userId, CancellationToken ct)
	{
		var result = await context
			.Trades.Where(trade =>
				trade.UserId == userId
				&& trade.ClosedAt.HasValue
				&& trade.ClosePrice.HasValue
				&& trade.OpenPrice != 0
				&& trade.Signal != TradeSignal.HOLD
			)
			.Select(trade => new { Income = trade.NetIncome!.Value })
			.GroupBy(_ => 1)
			.Select(group => new TradeStatisticsAggregate(
				group.Count(),
				group.Count(trade => trade.Income > 0),
				group.Count(trade => trade.Income < 0),
				group.Sum(trade => Math.Abs(trade.Income)),
				group.Where(trade => trade.Income > 0).Sum(trade => trade.Income),
				group.Where(trade => trade.Income < 0).Sum(trade => -trade.Income)
			))
			.SingleOrDefaultAsync(ct);

		return result ?? TradeStatisticsAggregate.Empty;
	}

	public async Task<int> ExecuteUpdateAsync(
		int userId,
		int id,
		int instrumentId,
		int tradeTypeId,
		DateTime openedAt,
		DateTime? closedAt,
		double openPrice,
		double? closePrice,
		TradeSignal signal,
		int quantity,
		decimal price,
		CancellationToken ct
	)
	{
		return await context
			.Trades.Where(t => t.Id == id && t.UserId == userId)
			.ExecuteUpdateAsync(
				s =>
					s.SetProperty(t => t.OpenedAt, openedAt)
						.SetProperty(t => t.ClosedAt, closedAt)
						.SetProperty(t => t.OpenPrice, openPrice)
						.SetProperty(t => t.ClosePrice, closePrice)
						.SetProperty(t => t.Quantity, quantity)
						.SetProperty(t => t.Signal, signal)
						.SetProperty(t => t.TotalPrice, price)
						.SetProperty(t => t.TradeTypeId, tradeTypeId)
						.SetProperty(t => t.InstrumentId, instrumentId),
				ct
			);
	}

	private IQueryable<Trade> GetClosedTradesQuery(int userId, DateOnly? startDate, DateOnly? endDate)
	{
		var query = context.Trades.Where(trade =>
			trade.UserId == userId
			&& trade.ClosedAt.HasValue
			&& trade.ClosePrice.HasValue
			&& trade.OpenPrice != 0
			&& trade.Signal != TradeSignal.HOLD
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
