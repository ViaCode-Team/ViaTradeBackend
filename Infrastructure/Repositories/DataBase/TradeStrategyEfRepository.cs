using Application.Common.Interfaces;
using Application.Common.Models.Pagination;
using Application.Strategies.Interfaces;
using Domain.Strategies.Entities;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.DataBase;

public class TradeStrategyEfRepository(AppDbContext context) : GenericEfRepository<TradeStrategy>(context),
	ITradeStrategyRepository
{
	public async Task<int> CountAsync(CancellationToken ct = default)
	{
		return await _dbSet.CountAsync(ct);
	}

	public async Task<TradeStrategy?> GetByNameAsync(string name, CancellationToken ct = default)
	{
		return await _dbSet
			.Where(tradeStrategy => tradeStrategy.Name == name)
			.FirstOrDefaultAsync(ct);
	}

	public async Task<PagedResult<TradeStrategy>> GetPagedFilteredAsync(int userId, IQuerySpecification<TradeStrategy> spec, PaginationRequest paginationRequest, CancellationToken ct = default)
	{
		var queryable = SpecificationEvaluator.GetQuery(_dbSet.AsQueryable(), spec);

		if (spec.SortExpressions.Count == 0)
			queryable = queryable.OrderBy(e => e.Id);

		var pagedTuple = await queryable
			.Select(tradeStrategy => new
			{
				Strategy = tradeStrategy,
				IsActive = tradeStrategy.UserTradeStrategies!.Any(uts => uts.UserId == userId)
			})
			.ToPagedAsync(paginationRequest, ct);

		return pagedTuple.Map(t =>
		{
			t.Strategy.IsActive = t.IsActive;
			return t.Strategy;
		});
	}
}
