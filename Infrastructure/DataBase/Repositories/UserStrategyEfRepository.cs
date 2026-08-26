using Microsoft.EntityFrameworkCore;
using ViaTrade.Application.Strategies.Interfaces;
using ViaTrade.Application.Trades.Models;
using ViaTrade.Domain.Entities;
using ViaTrade.Infrastructure.Utils;

namespace ViaTrade.Infrastructure.DataBase.Repositories;

public class UserStrategyEfRepository(AppDbContext context, EfQueryObjectBuilder queryObjectBuilder)
	: BaseEfRepository<UserStrategy>(context, queryObjectBuilder),
		IUserStrategyRepository
{
	public async Task<int> CountByUserAsync(int userId, CancellationToken ct)
	{
		return await _dbSet.CountAsync(e => e.UserId == userId, ct);
	}

	public Task ExecuteUnsubscribeAsync(int userId, int strategyId, CancellationToken ct)
	{
		return _dbSet.Where(link => link.UserId == userId && link.StrategyId == strategyId).ExecuteDeleteAsync(ct);
	}

	public async Task<List<SignalSourceDto>> ListSignalSourcesAsync(int userId, CancellationToken ct)
	{
		var userCodesQuery = _context.GetUserCodesQuery(userId);
		var allowedStrategiesQuery = _context.GetAllowedStrategiesQuery(userId);
		var projections = await userCodesQuery
			.FilterByAllowedStrategies(allowedStrategiesQuery)
			.ProjectToStrategyAndInstrument()
			.ToListAsync(ct);

		return projections
			.Select(source => new SignalSourceDto(
				source.StrategyId,
				source.StrategyName,
				source.DisplayName,
				source.InstrumentId,
				source.Symbol,
				source.Accuracy
			))
			.ToList();
	}
}
