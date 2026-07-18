using Application.Common.Models.Pagination;
using Application.Strategies.Interfaces;
using Domain.Strategies.Entities;
using Infrastructure.Utils;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.DataBase;

public class UserTradeStrategyEfRepository(AppDbContext context) : GenericEfRepository<UserTradeStrategy>(context),
	IUserTradeStrategyRepository
{
	public async Task<PagedResult<UserTradeStrategy>> GetByUserPagedAsync(int userId, PaginationRequest paginationRequest, CancellationToken cancellationToken)
	{
		return await FindPagedAsync(e => e.UserId == userId, paginationRequest, cancellationToken);
	}

	public async Task<int> CountByUserAsync(int userId, CancellationToken cancellationToken)
	{
		return await _context.UserTradeStrategies
			.CountAsync(e => e.UserId == userId, cancellationToken);
	}

	public async Task<Dictionary<string, List<string>>> GetUserPreferencesAsync(
		int userId,
		CancellationToken ct)
	{
		var userCodesQuery = _context.GetUserCodesQuery(userId);
		var allowedStrategiesQuery = _context.GetAllowedStrategiesQuery(userId);

		var projectedQuery = userCodesQuery
			.FilterByAllowedStrategies(allowedStrategiesQuery)
			.ProjectToStrategyAndTradeCode();

		var queryResults = await projectedQuery.ToListAsync(ct);

		return queryResults.GroupByStrategyName();
	}
}
