using Application.Notes.Interfaces;
using Application.Statistics.Models;
using Domain.Notes.Entities;
using Domain.Notes.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.DataBase;

public class NoteEfRepository(AppDbContext context)
	: GenericEfRepository<Note>(context), INoteRepository
{
	public async Task<NoteStatisticReadModel> GetNoteStatisticAsync(int userId, CancellationToken ct = default)
	{
		var baseQuery = _dbSet.Where(n => n.UserId == userId);

		var totalNotes = await baseQuery.CountAsync(ct);
		if (totalNotes == 0)
			return new NoteStatisticReadModel { TotalNotes = 0, StockNotes = 0, StrategyNotes = 0 };

		var stockNotes = await baseQuery.CountAsync(n => n.TradeCodeId != null, ct);
		var strategyNotes = await baseQuery.CountAsync(n => n.TradeStrategyId != null, ct);

		return new NoteStatisticReadModel
		{
			TotalNotes = totalNotes,
			StockNotes = stockNotes,
			StrategyNotes = strategyNotes
		};
	}

	public async Task<Note?> FindByTargetAsync(int userId, int relatedId, NoteType noteType, CancellationToken ct) => noteType switch
	{
		NoteType.TradeCodeNote => await _dbSet.FirstOrDefaultAsync(n => n.TradeCodeId == relatedId && n.UserId == userId, ct),
		NoteType.TradeStrategyNote => await _dbSet.FirstOrDefaultAsync(n => n.TradeStrategyId == relatedId && n.UserId == userId, ct),
		_ => null
	};

	public async Task<Note> GetByTargetAsync(int id, int userId, NoteType noteType, CancellationToken ct)
	{
		Note? found = noteType switch
		{
			NoteType.TradeCodeNote => await _dbSet.FirstOrDefaultAsync(n => n.TradeCodeId == id && n.UserId == userId, ct),
			NoteType.TradeStrategyNote => await _dbSet.FirstOrDefaultAsync(n => n.TradeStrategyId == id && n.UserId == userId, ct),
			_ => throw new KeyNotFoundException()
		};

		return found ?? throw new KeyNotFoundException();
	}

	public async Task AddUserNoteAsync(int relatedId, NoteType noteType, int userId, string noteText, CancellationToken ct)
	{
		int? tradeCodeId = noteType == NoteType.TradeCodeNote ? relatedId : null;
		int? tradeStrategyId = noteType == NoteType.TradeStrategyNote ? relatedId : null;

		var note = new Note
		{
			UserId = userId,
			NoteText = noteText,
			TradeCodeId = tradeCodeId,
			TradeStrategyId = tradeStrategyId
		};

		_dbSet.Add(note);
	}

	public async Task ExecuteUpdateUserNoteAsync(int id, NoteType noteType, int userId, string noteText, CancellationToken ct)
	{
		int affectedRows = noteType switch
		{
			NoteType.TradeCodeNote => await _dbSet
				.Where(n => n.TradeCodeId == id && n.UserId == userId)
				.ExecuteUpdateAsync(s => s.SetProperty(n => n.NoteText, noteText), ct),

			NoteType.TradeStrategyNote => await _dbSet
				.Where(n => n.TradeStrategyId == id && n.UserId == userId)
				.ExecuteUpdateAsync(s => s.SetProperty(n => n.NoteText, noteText), ct),

			_ => throw new KeyNotFoundException()
		};

		if (affectedRows == 0)
			throw new KeyNotFoundException();
	}

	public async Task DeleteUserNoteAsync(int id, int userId, NoteType noteType, CancellationToken ct)
	{
		int affectedRows = noteType switch
		{
			NoteType.TradeCodeNote => await _dbSet
				.Where(n => n.TradeCodeId == id && n.UserId == userId)
				.ExecuteDeleteAsync(ct),

			NoteType.TradeStrategyNote => await _dbSet
				.Where(n => n.TradeStrategyId == id && n.UserId == userId)
				.ExecuteDeleteAsync(ct),

			_ => throw new KeyNotFoundException()
		};

		if (affectedRows == 0)
			throw new KeyNotFoundException();
	}
}
