using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Trades.Interfaces;
using Application.Trades.Models;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DataBase.Repositories;

public class TradeEfRepository(AppDbContext context) : GenericEfRepository<Trade>(context), ITradeRepository
{
	private static readonly DateTime WeekEpoch = new(1900, 1, 1);

	public async Task<List<ProfitChartAggregateRow>> GetProfitChartAsync(
		int userId,
		ProfitChartFilter filter,
		CancellationToken ct
	)
	{
		var trades = await GetClosedTradesQuery(userId, filter.StartDate, filter.EndDate)
			.Select(trade => new
			{
				ClosedAt = trade.ClosedAt!.Value,
				trade.EntryPrice,
				ExitPrice = trade.ExitPrice!.Value,
				trade.Signal,
			})
			.ToListAsync(ct);

		return filter.Granularity switch
		{
			ProfitChartGranularity.Day => trades
				.GroupBy(trade => new
				{
					trade.ClosedAt.Year,
					trade.ClosedAt.Month,
					trade.ClosedAt.Day,
				})
				.Select(group => new ProfitChartAggregateRow(
					group.Key.Year,
					group.Key.Month,
					group.Key.Day,
					null,
					group.Sum(trade => CalculateProfitChartIncome(trade.EntryPrice, trade.ExitPrice, trade.Signal)),
					group
						.Where(trade => trade.Signal == TradeSignal.BUY)
						.Sum(trade => CalculateProfitChartIncome(trade.EntryPrice, trade.ExitPrice, trade.Signal)),
					group
						.Where(trade => trade.Signal == TradeSignal.SELL)
						.Sum(trade => CalculateProfitChartIncome(trade.EntryPrice, trade.ExitPrice, trade.Signal))
				))
				.OrderBy(row => row.Year)
				.ThenBy(row => row.Month)
				.ThenBy(row => row.Day)
				.ToList(),
			ProfitChartGranularity.Week => trades
				.GroupBy(trade => Math.Floor((trade.ClosedAt.Date - WeekEpoch).TotalDays / 7))
				.Select(group => new ProfitChartAggregateRow(
					null,
					null,
					null,
					group.Key,
					group.Sum(trade => CalculateProfitChartIncome(trade.EntryPrice, trade.ExitPrice, trade.Signal)),
					group
						.Where(trade => trade.Signal == TradeSignal.BUY)
						.Sum(trade => CalculateProfitChartIncome(trade.EntryPrice, trade.ExitPrice, trade.Signal)),
					group
						.Where(trade => trade.Signal == TradeSignal.SELL)
						.Sum(trade => CalculateProfitChartIncome(trade.EntryPrice, trade.ExitPrice, trade.Signal))
				))
				.OrderBy(row => row.WeekIndex)
				.ToList(),
			ProfitChartGranularity.Month => trades
				.GroupBy(trade => new { trade.ClosedAt.Year, trade.ClosedAt.Month })
				.Select(group => new ProfitChartAggregateRow(
					group.Key.Year,
					group.Key.Month,
					null,
					null,
					group.Sum(trade => CalculateProfitChartIncome(trade.EntryPrice, trade.ExitPrice, trade.Signal)),
					group
						.Where(trade => trade.Signal == TradeSignal.BUY)
						.Sum(trade => CalculateProfitChartIncome(trade.EntryPrice, trade.ExitPrice, trade.Signal)),
					group
						.Where(trade => trade.Signal == TradeSignal.SELL)
						.Sum(trade => CalculateProfitChartIncome(trade.EntryPrice, trade.ExitPrice, trade.Signal))
				))
				.OrderBy(row => row.Year)
				.ThenBy(row => row.Month)
				.ToList(),
			_ => throw new ArgumentOutOfRangeException(
				nameof(filter.Granularity),
				filter.Granularity,
				"Unsupported chart granularity."
			),
		};
	}

	private static double CalculateProfitChartIncome(double entryPrice, double exitPrice, TradeSignal signal)
	{
		return Math.Round((exitPrice - entryPrice) / entryPrice * 100 * (int)signal, 2);
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
			.Select(ToProjection())
			.FirstOrDefaultAsync(ct);
	}

	public async Task<PageResult<TradeProjectionDto>> GetPageProjectionAsync(
		IQuerySpecification<Trade> specification,
		PageOptions pageOptions,
		CancellationToken ct
	)
	{
		var query = SpecificationEvaluator.GetQuery(_dbSet, specification);
		if (specification.SortExpressions.Count == 0)
			query = query.OrderBy(trade => trade.Id);

		return await query.Select(ToProjection()).ToPagedAsync(pageOptions, ct);
	}

	public async Task<TradeStatisticAggregateDto> GetGlobalStatisticsAsync(int userId, CancellationToken ct)
	{
		var result = await _context
			.Trades.Where(trade =>
				trade.UserId == userId
				&& trade.ExitPrice.HasValue
				&& trade.EntryPrice != 0
				&& trade.Signal != TradeSignal.HOLD
			)
			.Select(trade => new
			{
				Income = Math.Round(
					(trade.ExitPrice!.Value - trade.EntryPrice) / trade.EntryPrice * 100 * (int)trade.Signal,
					2
				),
			})
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
							.SetProperty(t => t.EntryPrice, request.EntryPrice)
							.SetProperty(t => t.ExitPrice, request.ExitPrice)
							.SetProperty(t => t.Quantity, request.Quantity)
							.SetProperty(t => t.Signal, request.Signal)
							.SetProperty(t => t.TotalPrice, price)
							.SetProperty(t => t.TradeTypeId, request.TradeTypeId)
							.SetProperty(t => t.InstrumentId, request.InstrumentId),
					ct
				)
		);
	}

	private static System.Linq.Expressions.Expression<Func<Trade, TradeProjectionDto>> ToProjection()
	{
		return trade => new TradeProjectionDto(
			trade.Id,
			trade.OpenedAt,
			trade.ClosedAt,
			trade.EntryPrice,
			trade.ExitPrice,
			trade.Quantity,
			trade.TotalPrice,
			trade.Signal,
			trade.TradeTypeId,
			new InstrumentSummaryDto(trade.Instrument!.Id, trade.Instrument.Symbol, trade.Instrument.Description),
			trade.UserId
		);
	}

	private IQueryable<Trade> GetClosedTradesQuery(int userId, DateOnly? startDate, DateOnly? endDate)
	{
		var query = _context.Trades.Where(trade =>
			trade.UserId == userId
			&& trade.ClosedAt.HasValue
			&& trade.EntryPrice != 0
			&& trade.ExitPrice.HasValue
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
