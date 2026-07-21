using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Strategies.Interfaces;
using Application.Strategies.Models;
using Domain.Strategies.Entities;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DataBase.Repositories;

public class TradeStrategyEfRepository(AppDbContext context)
	: GenericEfRepository<TradeStrategy>(context),
		ITradeStrategyRepository
{
	public async Task<StrategyCountsDto> GetStatisticAsync(int userId, CancellationToken ct)
	{
		var query = _context
			.Users.Where(user => user.Id == userId)
			.Select(_ => new StrategyCountsDto(
				_context.TradeStrategies.LongCount(),
				_context.UserTradeStrategies.LongCount(link => link.UserId == userId)
			));

		return await query.SingleOrDefaultAsync(ct)
			?? throw new KeyNotFoundException($"User with ID {userId} was not found.");
	}

	public async Task<TradeStrategy?> GetByNameAsync(string name, CancellationToken ct = default)
	{
		return await _dbSet.Where(tradeStrategy => tradeStrategy.Name == name).FirstOrDefaultAsync(ct);
	}

	public async Task<PageResult<TradeStrategy>> GetPagedFilteredAsync(
		int userId,
		IQuerySpecification<TradeStrategy> spec,
		PageOptions page,
		CancellationToken ct = default
	)
	{
		var queryable = SpecificationEvaluator.GetQuery(_dbSet.AsQueryable(), spec);

		if (spec.SortExpressions.Count == 0)
			queryable = queryable.OrderBy(e => e.Id);

		var pagedTuple = await queryable
			.Select(tradeStrategy => new
			{
				Strategy = tradeStrategy,
				IsActive = tradeStrategy.UserTradeStrategies!.Any(uts => uts.UserId == userId),
			})
			.ToPagedAsync(page, ct);

		return pagedTuple.Map(t =>
		{
			t.Strategy.IsActive = t.IsActive;
			return t.Strategy;
		});
	}
}
