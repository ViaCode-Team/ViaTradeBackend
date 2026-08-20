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

public class UserStrategyInstrumentEfRepository(AppDbContext context)
	: BaseEfRepository<UserStrategyInstrument>(context),
		IUserStrategyInstrumentRepository
{
	public async Task<PageResult<StrategySubscriptionDto>> GetStrategiesPageByInstrumentAsync(
		int userId,
		IQueryObject<UserStrategyInstrument> queryObject,
		PageOptions pageOptions,
		CancellationToken ct
	)
	{
		var query = QueryObjectEvaluator.GetQueryForPagination(_dbSet.AsQueryable(), queryObject);
		var strategyQuery = query.Select(link => link.Strategy!);

		return await strategyQuery.WithSubscriptionState(userId).ToPagedAsync(pageOptions, ct);
	}

	public async Task<StrategyInstrumentsPageResult> GetInstrumentsPageByStrategyAsync(
		int strategyId,
		IQueryObject<UserStrategyInstrument> queryObject,
		PageOptions pageOptions,
		CancellationToken ct
	)
	{
		var linksQuery = QueryObjectEvaluator.GetQueryForPagination(_dbSet.AsQueryable(), queryObject);

		var strategyPageInfo = await _context
			.Strategies.Where(strategy => strategy.Id == strategyId)
			.Select(_ => new { TotalCount = linksQuery.Count() })
			.SingleOrDefaultAsync(ct);

		if (strategyPageInfo == null)
		{
			return new StrategyInstrumentsPageResult(
				false,
				new PageResult<RelatedInstrumentDto>([], 0, pageOptions.Page, pageOptions.PageSize)
			);
		}

		var query = linksQuery.Select(link => link.Instrument!);

		var instruments = await query
			.Select(instrument => new RelatedInstrumentDto(instrument.Id, instrument.Symbol, instrument.Description))
			.ToPagedAsync(pageOptions, strategyPageInfo.TotalCount, ct);

		return new StrategyInstrumentsPageResult(true, instruments);
	}
}
