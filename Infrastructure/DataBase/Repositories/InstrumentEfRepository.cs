using Application.Common.Models;
using Application.Instruments.Interfaces;
using Application.Instruments.Models;
using Domain.Entities;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DataBase.Repositories;

public class InstrumentEfRepository(AppDbContext context) : BaseEfRepository<Instrument>(context), IInstrumentRepository
{
	public async Task<Instrument?> FindByTickerAsync(string ticker, CancellationToken ct)
	{
		return await _dbSet.Where(e => e.Symbol == ticker).FirstOrDefaultAsync(ct);
	}

	public async Task<int?> FindIdByTickerAsync(string ticker, CancellationToken ct)
	{
		return await _dbSet.Where(e => e.Symbol == ticker).Select(e => (int?)e.Id).FirstOrDefaultAsync(ct);
	}

	public async Task<string?> FindTickerByIdAsync(int instrumentId, CancellationToken ct)
	{
		return await _dbSet.Where(e => e.Id == instrumentId).Select(e => e.Symbol).FirstOrDefaultAsync(ct);
	}

	public async Task<Dictionary<string, int>> GetInstrumentIdByTickerAsync(CancellationToken ct)
	{
		return await _dbSet
			.Select(instrument => new InstrumentReferenceDto(instrument.Id, instrument.Symbol))
			.ToDictionaryAsync(
				instrument => instrument.Symbol,
				instrument => instrument.Id,
				StringComparer.OrdinalIgnoreCase,
				ct
			);
	}

	public async Task<PageResult<Instrument>> GetPageAsync(
		InstrumentFilter instrumentFilter,
		PageOptions pageOptions,
		InstrumentSort instrumentSort,
		CancellationToken ct
	)
	{
		var query = _dbSet.AsQueryable();
		if (!string.IsNullOrWhiteSpace(instrumentFilter.Symbol))
		{
			var pattern = $"%{instrumentFilter.Symbol}%";
			query = query.Where(instrument => EF.Functions.Like(instrument.Symbol, pattern));
		}

		query = ApplySort(query, instrumentSort);

		return await query.ToPagedAsync(pageOptions, ct);
	}

	private static IQueryable<Instrument> ApplySort(IQueryable<Instrument> query, InstrumentSort instrumentSort)
	{
		var sortFields = instrumentSort.GetEffectiveSortBy();

		if (sortFields.Count > 0)
		{
			IOrderedQueryable<Instrument>? orderedQuery = null;
			foreach (var field in sortFields)
			{
				if (orderedQuery == null)
				{
					orderedQuery = field switch
					{
						InstrumentSortField.SymbolDesc => query.OrderByDescending(e => e.Symbol),
						_ => query.OrderBy(e => e.Symbol),
					};
				}
				else
				{
					orderedQuery = field switch
					{
						InstrumentSortField.SymbolDesc => orderedQuery.ThenByDescending(e => e.Symbol),
						_ => orderedQuery.ThenBy(e => e.Symbol),
					};
				}
			}
			query = orderedQuery ?? query;
		}
		else
		{
			query = query.OrderBy(e => e.Id);
		}

		return query;
	}
}
