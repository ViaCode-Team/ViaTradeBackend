using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Common.Abstractions.Repositories;
using ViaTrade.Application.Common.Exceptions;
using ViaTrade.Application.Notes.Common;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Notes.Get;

public sealed class GetNoteHandler(IReadRepository<Note> noteRepository) : IQueryHandler<GetNoteQuery, NoteResult>
{
	public async Task<NoteResult> HandleAsync(GetNoteQuery query, CancellationToken ct = default)
	{
		var note = await noteRepository.FirstOrDefaultAsync(
			note => note.UserId == query.UserId && note.Id == query.NoteId,
			NoteResult.Projection,
			ct
		);
		if (note == null)
			throw new NotFoundException("Note not found.", "note_not_found");

		return note;
	}
}
