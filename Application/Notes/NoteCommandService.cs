using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Notes.Interfaces;
using Domain.Notes.Entities;
using Domain.Notes.Enums;

namespace Application.Notes;

public class NoteCommandService(INoteRepository noteRepository, IUnitOfWork uow) : INoteCommandService
{
	public async Task AddAsync(int userId, int relatedId, NoteType noteType, string noteText, CancellationToken ct)
	{
		var note = noteType switch
		{
			NoteType.TradeCodeNote => new Note
			{
				UserId = userId,
				NoteText = noteText,
				TradeCodeId = relatedId,
			},

			NoteType.TradeStrategyNote => new Note
			{
				UserId = userId,
				NoteText = noteText,
				TradeStrategyId = relatedId,
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
				&& (noteType == NoteType.TradeCodeNote ? x.TradeCodeId == relatedId : x.TradeStrategyId == relatedId),
			ct
		);
	}

	public async Task UpdateAsync(int userId, int relatedId, NoteType noteType, string noteText, CancellationToken ct)
	{
		await noteRepository.ExecuteUpdateUserNoteAsync(userId, relatedId, noteType, noteText, ct);
	}
}
