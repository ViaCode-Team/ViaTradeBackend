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
}
