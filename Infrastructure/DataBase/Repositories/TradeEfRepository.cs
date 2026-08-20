using Microsoft.EntityFrameworkCore;
using ViaTrade.Application.Common.Interfaces;
using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Trades.Interfaces;
using ViaTrade.Application.Trades.Models;
using ViaTrade.Domain.Entities;
using ViaTrade.Domain.Enums;
using ViaTrade.Infrastructure.DataBase.Extensions;

namespace ViaTrade.Infrastructure.DataBase.Repositories;

public class TradeEfRepository(AppDbContext context, EfQueryObjectBuilder queryObjectBuilder)
	: BaseEfRepository<Trade>(context, queryObjectBuilder),
		ITradeRepository
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
			.Select(trade => new TradeProjectionDto(
				trade.Id,
				trade.OpenedAt,
				trade.ClosedAt,
				trade.OpenPrice,
				trade.ClosePrice,
				trade.NetIncome,
				trade.Quantity,
				trade.TotalPrice,
				trade.Signal,
				trade.TradeTypeId,
				new InstrumentSummaryDto(trade.Instrument!.Id, trade.Instrument.Symbol, trade.Instrument.Description),
				trade.UserId
			))
			.FirstOrDefaultAsync(ct);
	}

	public async Task<PageResult<TradeProjectionDto>> GetPageProjectionAsync(
		IQueryObject<Trade> queryObject,
		PageOptions pageOptions,
		CancellationToken ct
	)
	{
		var (query, isUnique) = _queryObjectBuilder.BuildForPagination(_dbSet.AsQueryable(), queryObject);

		var projectedQuery = query.Select(trade => new TradeProjectionDto(
			trade.Id,
			trade.OpenedAt,
			trade.ClosedAt,
			trade.OpenPrice,
			trade.ClosePrice,
			trade.NetIncome,
			trade.Quantity,
			trade.TotalPrice,
			trade.Signal,
			trade.TradeTypeId,
			new InstrumentSummaryDto(trade.Instrument!.Id, trade.Instrument.Symbol, trade.Instrument.Description),
			trade.UserId
		));

		return await projectedQuery.ToPagedAsync(pageOptions, isUnique, ct);
	}

	public async Task<TradeStatisticAggregateDto> GetGlobalStatisticsAsync(int userId, CancellationToken ct)
	{
		var result = await _context
			.Trades.Where(trade =>
				trade.UserId == userId
				&& trade.ClosedAt.HasValue
				&& trade.ClosePrice.HasValue
				&& trade.OpenPrice != 0
				&& trade.Signal != TradeSignal.HOLD
			)
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
