using Microsoft.EntityFrameworkCore;
using ViaTrade.Application.Common.Interfaces;
using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Notes.Interfaces;
using ViaTrade.Application.Notes.Models;
using ViaTrade.Domain.Entities;
using ViaTrade.Infrastructure.DataBase.Extensions;

namespace ViaTrade.Infrastructure.DataBase.Repositories;

public class NoteEfRepository(AppDbContext context) : BaseEfRepository<Note>(context), INoteRepository
{
	public async Task<NoteStatisticDto> GetStatisticsAsync(int userId, CancellationToken ct)
	{
		var statistics = await _dbSet
			.Where(note => note.UserId == userId)
			.GroupBy(_ => 1)
			.Select(group => new NoteStatisticDto(
				group.Count(),
				group.Count(note => note.InstrumentId != null),
				group.Count(note => note.StrategyId != null)
			))
			.SingleOrDefaultAsync(ct);

		return statistics ?? new NoteStatisticDto(0, 0, 0);
	}

	public async Task<PageResult<NoteProjectionDto>> GetPageWithTargetsAsync(
		IQueryObject<Note> queryObject,
		PageOptions pageOptions,
		CancellationToken ct
	)
	{
		var query = QueryObjectEvaluator.GetQueryForPagination(_dbSet, queryObject, _entityType);

		return await query
			.Select(note => new NoteProjectionDto(
				note.Id,
				note.Text,
				note.UserId,
				note.InstrumentId,
				note.Instrument!.Symbol,
				note.Instrument!.Description,
				note.StrategyId,
				note.Strategy!.Name,
				note.Strategy.DisplayName,
				note.Strategy!.Description
			))
			.ToPagedAsync(pageOptions, ct);
	}

	public async Task<Note?> FindByIdForUserAsync(int userId, int noteId, CancellationToken ct)
	{
		return await _dbSet
			.Include(note => note.Instrument)
			.Include(note => note.Strategy)
			.FirstOrDefaultAsync(note => note.Id == noteId && note.UserId == userId, ct);
	}

	public async Task<Note?> FindByInstrumentAsync(int userId, int instrumentId, CancellationToken ct)
	{
		return await _dbSet
			.Include(note => note.Instrument)
			.FirstOrDefaultAsync(note => note.InstrumentId == instrumentId && note.UserId == userId, ct);
	}

	public async Task<Note?> FindByStrategyAsync(int userId, int strategyId, CancellationToken ct)
	{
		return await _dbSet
			.Include(note => note.Strategy)
			.FirstOrDefaultAsync(note => note.StrategyId == strategyId && note.UserId == userId, ct);
	}

	public Task<int> ExecuteDeleteInstrumentAsync(int userId, int instrumentId, CancellationToken ct)
	{
		return EfDatabaseOperation.ExecuteAsync(() =>
			_dbSet.Where(note => note.UserId == userId && note.InstrumentId == instrumentId).ExecuteDeleteAsync(ct)
		);
	}

	public Task<int> ExecuteDeleteStrategyAsync(int userId, int strategyId, CancellationToken ct)
	{
		return EfDatabaseOperation.ExecuteAsync(() =>
			_dbSet.Where(note => note.UserId == userId && note.StrategyId == strategyId).ExecuteDeleteAsync(ct)
		);
	}

	public Task<int> ExecuteUpdateInstrumentAsync(int userId, int instrumentId, string text, CancellationToken ct)
	{
		return EfDatabaseOperation.ExecuteAsync(() =>
			_dbSet
				.Where(note => note.UserId == userId && note.InstrumentId == instrumentId)
				.ExecuteUpdateAsync(setters => setters.SetProperty(note => note.Text, text), ct)
		);
	}

	public Task<int> ExecuteUpdateStrategyAsync(int userId, int strategyId, string text, CancellationToken ct)
	{
		return EfDatabaseOperation.ExecuteAsync(() =>
			_dbSet
				.Where(note => note.UserId == userId && note.StrategyId == strategyId)
				.ExecuteUpdateAsync(setters => setters.SetProperty(note => note.Text, text), ct)
		);
	}
}
