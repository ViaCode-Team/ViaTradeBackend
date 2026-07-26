using Application.Common.Models;
using Application.Strategies.Interfaces;
using Application.Trades.Models;
using Domain.Strategies.Entities;
using Infrastructure.Utils;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DataBase.Repositories;

public class UserTradeStrategyEfRepository(AppDbContext context)
	: GenericEfRepository<UserTradeStrategy>(context),
		IUserTradeStrategyRepository
{
	public async Task<int> CountByUserAsync(int userId, CancellationToken ct)
	{
		return await _dbSet.CountAsync(e => e.UserId == userId, ct);
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
				source.InstrumentId,
				source.Symbol,
				source.Accuracy
			))
			.ToList();
	}
}
