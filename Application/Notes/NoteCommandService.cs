using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Notes.Interfaces;
using Domain.Notes.Entities;
using Domain.Notes.Enums;

namespace Application.Notes;

public class NoteCommandService(INoteRepository noteRepository, IUnitOfWork uow) : INoteCommandService
{
	public async Task AddAsync(int relatedId, NoteType noteType, int userId, string noteText, CancellationToken ct)
	{
		var existingNote = await noteRepository.FirstOrDefaultAsync(
			x =>
				x.UserId == userId
				&& (noteType == NoteType.TradeCodeNote ? x.TradeCodeId == relatedId : x.TradeStrategyId == relatedId),
			ct
		);

		if (existingNote != null)
			throw new ConflictException("Note already exists. Use update.", "note_already_exists");

		var note = new Note
		{
			UserId = userId,
			NoteText = noteText,
			TradeCodeId = noteType == NoteType.TradeCodeNote ? relatedId : null,
			TradeStrategyId = noteType == NoteType.TradeStrategyNote ? relatedId : null,
		};

		await noteRepository.AddAsync(note, ct);
		await uow.SaveChangesAsync(ct);
	}

	public async Task DeleteAsync(int relatedId, int userId, NoteType noteType, CancellationToken ct)
	{
		var rows = await noteRepository.ExecuteDeleteAsync(
			x =>
				x.UserId == userId
				&& (noteType == NoteType.TradeCodeNote ? x.TradeCodeId == relatedId : x.TradeStrategyId == relatedId),
			ct
		);

		if (rows == 0)
			throw new NotFoundException("Note not found.", "note_not_found");
	}

	public async Task UpdateAsync(int relatedId, NoteType noteType, int userId, string noteText, CancellationToken ct)
	{
		try
		{
			await noteRepository.ExecuteUpdateUserNoteAsync(relatedId, noteType, userId, noteText, ct);
		}
		catch (KeyNotFoundException exception)
		{
			throw new NotFoundException("Note not found.", "note_not_found", exception);
		}
	}
}
