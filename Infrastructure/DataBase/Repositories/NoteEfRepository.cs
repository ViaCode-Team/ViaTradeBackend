using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Notes.Interfaces;
using Application.Notes.Models;
using Domain.Notes.Entities;
using Domain.Notes.Enums;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DataBase.Repositories;

public class NoteEfRepository(AppDbContext context) : GenericEfRepository<Note>(context), INoteRepository
{
	public async Task<NoteStatisticDto> GetStatisticsAsync(int userId, CancellationToken ct = default)
	{
		var baseQuery = _dbSet.Where(n => n.UserId == userId);

		var totalNotes = await baseQuery.CountAsync(ct);
		if (totalNotes == 0)
			return new NoteStatisticDto(0, 0, 0);

		var stockNotes = await baseQuery.CountAsync(n => n.TradeCodeId != null, ct);
		var strategyNotes = await baseQuery.CountAsync(n => n.TradeStrategyId != null, ct);

		return new NoteStatisticDto(totalNotes, stockNotes, strategyNotes);
	}

	public async Task<PageResult<NoteProjectionDto>> GetPageWithTargetsAsync(
		IQuerySpecification<Note> specification,
		PageOptions pageOptions,
		CancellationToken ct
	)
	{
		var query = SpecificationEvaluator.GetQuery(_dbSet, specification);

		return await query
			.Select(note => new NoteProjectionDto(
				note.Id,
				note.NoteText,
				note.UserId,
				note.TradeCodeId,
				note.TradeCode!.ExchangeId,
				note.TradeCode!.Description,
				note.TradeStrategyId,
				note.TradeStrategy!.Name,
				note.TradeStrategy!.Description
			))
			.ToPagedAsync(pageOptions, ct);
	}

	public async Task<Note?> FindByTargetAsync(int userId, int relatedId, NoteType noteType, CancellationToken ct) =>
		noteType switch
		{
			NoteType.TradeCodeNote => await _dbSet
				.Include(note => note.TradeCode)
				.FirstOrDefaultAsync(note => note.TradeCodeId == relatedId && note.UserId == userId, ct),
			NoteType.TradeStrategyNote => await _dbSet
				.Include(note => note.TradeStrategy)
				.FirstOrDefaultAsync(note => note.TradeStrategyId == relatedId && note.UserId == userId, ct),
			_ => null,
		};

	public async Task AddUserNoteAsync(
		int relatedId,
		NoteType noteType,
		int userId,
		string noteText,
		CancellationToken ct
	)
	{
		int? tradeCodeId = noteType == NoteType.TradeCodeNote ? relatedId : null;
		int? tradeStrategyId = noteType == NoteType.TradeStrategyNote ? relatedId : null;

		var note = new Note
		{
			UserId = userId,
			NoteText = noteText,
			TradeCodeId = tradeCodeId,
			TradeStrategyId = tradeStrategyId,
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
			query.ExecuteUpdateAsync(setters => setters.SetProperty(note => note.NoteText, noteText), ct)
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
			NoteType.TradeCodeNote => _dbSet.Where(note => note.TradeCodeId == id && note.UserId == userId),

			NoteType.TradeStrategyNote => _dbSet.Where(note => note.TradeStrategyId == id && note.UserId == userId),

			_ => throw new ArgumentOutOfRangeException(nameof(noteType), noteType, "Unsupported note type."),
		};
	}
}
