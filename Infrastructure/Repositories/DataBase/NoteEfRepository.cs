using Application.Notes.Interfaces;
using Application.Statistics.Models;
using Domain.Notes.Entities;
using Domain.Notes.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.DataBase;

public class NoteEfRepository(AppDbContext context) : GenericEfRepository<Note>(context), INoteRepository
{
	public async Task<NoteStatisticReadModel> GetNoteStatisticAsync(int userId, CancellationToken cancellationToken = default)
	{
		var baseQuery = _dbSet.Where(n => n.UserId == userId);

		var totalNotes = await baseQuery.CountAsync(cancellationToken);
		if (totalNotes == 0)
			return new NoteStatisticReadModel { TotalNotes = 0, StockNotes = 0, StrategyNotes = 0 };

		var stockNotes = await baseQuery.CountAsync(n => n.TradeCodeId != null, cancellationToken);
		var strategyNotes = await baseQuery.CountAsync(n => n.TradeStrategyId != null, cancellationToken);

		return new NoteStatisticReadModel
		{
			TotalNotes = totalNotes,
			StockNotes = stockNotes,
			StrategyNotes = strategyNotes
		};
	}

	public async Task<Note?> FindUserNoteByEntityAsync(int userId, int relatedId, NoteType noteType, CancellationToken cancellationToken) => noteType switch
	{
		NoteType.TradeCodeNote => await _dbSet.FirstOrDefaultAsync(n => n.TradeCodeId == relatedId && n.UserId == userId, cancellationToken),
		NoteType.TradeStrategyNote => await _dbSet.FirstOrDefaultAsync(n => n.TradeStrategyId == relatedId && n.UserId == userId, cancellationToken),
		_ => null
	};

	public async Task<Note> GetUserNoteByProp(int id, int userId, NoteType noteType, CancellationToken cancellationToken)
	{
		Note? found = noteType switch
		{
			NoteType.TradeCodeNote => await _dbSet.FirstOrDefaultAsync(n => n.TradeCodeId == id && n.UserId == userId, cancellationToken),
			NoteType.TradeStrategyNote => await _dbSet.FirstOrDefaultAsync(n => n.TradeStrategyId == id && n.UserId == userId, cancellationToken),
			_ => throw new KeyNotFoundException()
		};

		return found ?? throw new KeyNotFoundException();
	}

	public async Task AddUserNoteAsync(int relatedId, NoteType noteType, int userId, string noteText, CancellationToken cancellationToken)
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

	public async Task UpdateUserNoteAsync(int id, NoteType noteType, int userId, string noteText, CancellationToken cancellationToken)
	{
		int affectedRows = noteType switch
		{
			NoteType.TradeCodeNote => await _dbSet
				.Where(n => n.TradeCodeId == id && n.UserId == userId)
				.ExecuteUpdateAsync(s => s.SetProperty(n => n.NoteText, noteText), cancellationToken),

			NoteType.TradeStrategyNote => await _dbSet
				.Where(n => n.TradeStrategyId == id && n.UserId == userId)
				.ExecuteUpdateAsync(s => s.SetProperty(n => n.NoteText, noteText), cancellationToken),

			_ => throw new KeyNotFoundException()
		};

		if (affectedRows == 0)
			throw new KeyNotFoundException();
	}

	public async Task DeleteUserNoteAsync(int id, int userId, NoteType noteType, CancellationToken cancellationToken)
	{
		int affectedRows = noteType switch
		{
			NoteType.TradeCodeNote => await _dbSet
				.Where(n => n.TradeCodeId == id && n.UserId == userId)
				.ExecuteDeleteAsync(cancellationToken),

			NoteType.TradeStrategyNote => await _dbSet
				.Where(n => n.TradeStrategyId == id && n.UserId == userId)
				.ExecuteDeleteAsync(cancellationToken),

			_ => throw new KeyNotFoundException()
		};

		if (affectedRows == 0)
			throw new KeyNotFoundException();
	}
}
