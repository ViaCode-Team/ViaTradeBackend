using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Notes.Interfaces;
using Application.Notes.Models;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DataBase.Repositories;

public class NoteEfRepository(AppDbContext context) : GenericEfRepository<Note>(context), INoteRepository
{
	public async Task<NoteStatisticDto> GetStatisticsAsync(int userId, CancellationToken ct = default)
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
		IQuerySpecification<Note> specification,
		PageOptions pageOptions,
		CancellationToken ct
	)
	{
		var query = SpecificationEvaluator.GetQuery(_dbSet, specification);
		if (specification.SortExpressions.Count == 0)
			query = query.OrderBy(note => note.Id);

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
				note.Strategy!.Description
			))
			.ToPagedAsync(pageOptions, ct);
	}

	public async Task<Note?> FindByTargetAsync(int userId, int relatedId, NoteType noteType, CancellationToken ct) =>
		noteType switch
		{
			NoteType.InstrumentNote => await _dbSet
				.Include(note => note.Instrument)
				.FirstOrDefaultAsync(note => note.InstrumentId == relatedId && note.UserId == userId, ct),
			NoteType.StrategyNote => await _dbSet
				.Include(note => note.Strategy)
				.FirstOrDefaultAsync(note => note.StrategyId == relatedId && note.UserId == userId, ct),
			_ => null,
		};

	public async Task<Note?> FindByIdForUserAsync(int userId, int noteId, CancellationToken ct)
	{
		return await _dbSet
			.Include(note => note.Instrument)
			.Include(note => note.Strategy)
			.FirstOrDefaultAsync(note => note.Id == noteId && note.UserId == userId, ct);
	}

	public async Task AddUserNoteAsync(
		int relatedId,
		NoteType noteType,
		int userId,
		string noteText,
		CancellationToken ct
	)
	{
		int? instrumentId = noteType == NoteType.InstrumentNote ? relatedId : null;
		int? strategyId = noteType == NoteType.StrategyNote ? relatedId : null;

		var note = new Note
		{
			UserId = userId,
			Text = noteText,
			InstrumentId = instrumentId,
			StrategyId = strategyId,
		};

		_dbSet.Add(note);
	}

	public async Task<int> ExecuteUpdateUserNoteAsync(
		int userId,
		int id,
		NoteType noteType,
		string noteText,
		CancellationToken ct
	)
	{
		var query = GetTargetQuery(id, userId, noteType);

		return await EfDatabaseOperation.ExecuteAsync(() =>
			query.ExecuteUpdateAsync(setters => setters.SetProperty(note => note.Text, noteText), ct)
		);
	}

	public async Task<int> ExecuteDeleteUserNoteAsync(int userId, int id, NoteType noteType, CancellationToken ct)
	{
		var query = GetTargetQuery(id, userId, noteType);

		return await query.ExecuteDeleteAsync(ct);
	}

	private IQueryable<Note> GetTargetQuery(int id, int userId, NoteType noteType)
	{
		return noteType switch
		{
			NoteType.InstrumentNote => _dbSet.Where(note => note.InstrumentId == id && note.UserId == userId),

			NoteType.StrategyNote => _dbSet.Where(note => note.StrategyId == id && note.UserId == userId),

			_ => throw new ArgumentOutOfRangeException(nameof(noteType), noteType, "Unsupported note type."),
		};
	}
}
