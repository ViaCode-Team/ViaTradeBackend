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
	public async Task<int> CountAsync(CancellationToken cancellationToken = default)
	{
		return await _dbSet.CountAsync(cancellationToken);
	}

	public async Task<TradeStrategy?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
	{
		return await _dbSet
			.Where(tradeStrategy => tradeStrategy.Name == name)
			.FirstOrDefaultAsync(cancellationToken);
	}

	public async Task<PagedResult<TradeStrategy>> GetPagedFilteredAsync(int userId, IQuerySpecification<TradeStrategy> spec, PaginationRequest? paginationRequest, CancellationToken cancellationToken = default)
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
			.ToPagedAsync(paginationRequest, cancellationToken);

		return pagedTuple.Map(t =>
		{
			t.Strategy.SetActive(t.IsActive);
			return t.Strategy;
		});
	}
}
