using Application.Common.Models;
using Application.Trades.Interfaces;
using Application.Trades.Models;
using Domain.Entities;
using Domain.Trades.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DataBase.Repositories;

public class TradeEfRepository(AppDbContext context) : GenericEfRepository<Trade>(context), ITradeRepository
{
	public async Task<TradeStatisticAggregateDto> GetGlobalStatisticAsync(int userId, CancellationToken ct)
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
				Income =
					Math.Round(
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

	public async Task<PageResult<Trade>> GetByUserPagedAsync(int userId, PageOptions page, CancellationToken ct)
	{
		return await FindPagedAsync(t => t.UserId == userId, page, ct);
	}

	public async Task<PageResult<Trade>> GetByUserAndTradeCodePagedAsync(
		int userId,
		int tradeCodeId,
		PageOptions page,
		CancellationToken ct
	)
	{
		return await FindPagedAsync(t => t.UserId == userId && t.TradeCodeId == tradeCodeId, page, ct);
	}

	public async Task<int> ExecuteUpdateAsync(
		int id,
		int userId,
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
}
