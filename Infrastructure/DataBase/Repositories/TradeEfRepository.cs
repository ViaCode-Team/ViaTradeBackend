using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Trades.Interfaces;
using Application.Trades.Models;
using Domain.Entities;
using Domain.Trades.Enums;
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
				&& trade.TradeClose.HasValue
				&& trade.TradeOpen != 0
				&& trade.TradeSignal != TradeSignal.HOLD
			)
			.Select(trade => new
			{
				Income = Math.Round(
					(trade.TradeClose!.Value - trade.TradeOpen) / trade.TradeOpen * 100 * (int)trade.TradeSignal,
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
						s.SetProperty(t => t.DateOpen, request.DateOpen)
							.SetProperty(t => t.DateClose, request.DateClose)
							.SetProperty(t => t.TradeOpen, request.TradeOpen)
							.SetProperty(t => t.TradeClose, request.TradeClose)
							.SetProperty(t => t.Count, request.Count)
							.SetProperty(t => t.TradeSignal, request.TradeSignal)
							.SetProperty(t => t.Price, price)
							.SetProperty(t => t.TradeTypeId, request.TradeTypeId)
							.SetProperty(t => t.TradeCodeId, request.TradeCodeId),
					ct
				)
		);
	}

	private static System.Linq.Expressions.Expression<Func<Trade, TradeProjectionDto>> ToProjection()
	{
		return trade => new TradeProjectionDto(
			trade.Id,
			trade.DateOpen,
			trade.DateClose,
			trade.TradeOpen,
			trade.TradeClose,
			trade.Count,
			trade.Price,
			trade.TradeSignal,
			trade.TradeTypeId,
			new TradeCodeSummaryDto(trade.TradeCode!.Id, trade.TradeCode.ExchangeId, trade.TradeCode.Description),
			trade.UserId
		);
	}
}
