using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Notes.Interfaces;
using Domain.Entities;
using Domain.Enums;

namespace Application.Notes;

public class NoteCommandService(INoteRepository noteRepository, IUnitOfWork uow) : INoteCommandService
{
	public async Task UpsertAsync(int userId, int relatedId, NoteType noteType, string noteText, CancellationToken ct)
	{
		var affectedRows = await noteRepository.ExecuteUpdateUserNoteAsync(userId, relatedId, noteType, noteText, ct);
		if (affectedRows != 0)
			return;

		var note = noteType switch
		{
			NoteType.InstrumentNote => new Note
			{
				UserId = userId,
				Text = noteText,
				InstrumentId = relatedId,
			},

			NoteType.StrategyNote => new Note
			{
				UserId = userId,
				Text = noteText,
				StrategyId = relatedId,
			},

			_ => throw new BadRequestException("Unsupported note target.", "invalid_note_target"),
		};

		await noteRepository.AddAsync(note, ct);
		await uow.SaveChangesAsync(ct);
	}

	public async Task DeleteAsync(int userId, int relatedId, NoteType noteType, CancellationToken ct)
	{
		var affectedRows = await noteRepository.ExecuteDeleteAsync(
			x =>
				x.UserId == userId
				&& (noteType == NoteType.InstrumentNote ? x.InstrumentId == relatedId : x.StrategyId == relatedId),
			ct
		);
		if (affectedRows == 0)
			throw new NotFoundException("Note not found.", "note_not_found");
	}
}
