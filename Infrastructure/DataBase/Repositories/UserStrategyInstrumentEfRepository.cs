using Microsoft.EntityFrameworkCore;
using ViaTrade.Application.Common.Interfaces;
using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Instruments.Models;
using ViaTrade.Application.Strategies.Interfaces;
using ViaTrade.Application.Strategies.Models;
using ViaTrade.Domain.Entities;
using ViaTrade.Infrastructure.DataBase.Extensions;
using ViaTrade.Infrastructure.Extensions;

namespace ViaTrade.Infrastructure.DataBase.Repositories;

public class UserStrategyInstrumentEfRepository(AppDbContext context, EfQueryObjectBuilder queryObjectBuilder)
	: BaseEfRepository<UserStrategyInstrument>(context, queryObjectBuilder),
		IUserStrategyInstrumentRepository
{
	public async Task<PageResult<StrategySubscriptionDto>> GetStrategiesPageByInstrumentAsync(
		int userId,
		IQueryObject<UserStrategyInstrument> queryObject,
		PageOptions pageOptions,
		CancellationToken ct
	)
	{
		var (query, isUnique) = _queryObjectBuilder.BuildForPagination(_dbSet.AsQueryable(), queryObject);
		var strategyQuery = query.Select(link => link.Strategy!);

		var projectedQuery = strategyQuery.WithSubscriptionState(userId);

		return await projectedQuery.ToPagedAsync(pageOptions, isUnique, ct);
	}

	public async Task<StrategyInstrumentsPageResult> GetInstrumentsPageByStrategyAsync(
		int strategyId,
		IQueryObject<UserStrategyInstrument> queryObject,
		PageOptions pageOptions,
		CancellationToken ct
	)
	{
		var (linksQuery, isUnique) = _queryObjectBuilder.BuildForPagination(_dbSet.AsQueryable(), queryObject);

		var strategyExists = await _context.Strategies.AnyAsync(strategy => strategy.Id == strategyId, ct);
		if (!strategyExists)
		{
			return new StrategyInstrumentsPageResult(
				false,
				new PageResult<RelatedInstrumentDto>([], 0, pageOptions.Page, pageOptions.PageSize)
			);
		}

		var query = linksQuery.Select(link => link.Instrument!);

		var projectedQuery = query.Select(instrument => new RelatedInstrumentDto(
			instrument.Id,
			instrument.Symbol,
			instrument.Description
		));

		var instruments = await projectedQuery.ToPagedAsync(pageOptions, isUnique, ct);

		return new StrategyInstrumentsPageResult(true, instruments);
	}
}
