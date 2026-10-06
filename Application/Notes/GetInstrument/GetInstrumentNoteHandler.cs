using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Common.Abstractions.Repositories;
using ViaTrade.Application.Common.Exceptions;
using ViaTrade.Application.Notes.Common;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Notes.GetInstrument;

public sealed class GetInstrumentNoteHandler(
	IReadRepository<Instrument> instrumentRepository,
	IReadRepository<Note> noteRepository
) : IQueryHandler<GetInstrumentNoteQuery, NoteResult>
{
	public async Task<NoteResult> HandleAsync(GetInstrumentNoteQuery query, CancellationToken ct = default)
	{
		var instrumentExists = await instrumentRepository.AnyAsync(
			instrument => instrument.Id == query.InstrumentId,
			ct
		);
		if (!instrumentExists)
			throw new NotFoundException("Instrument not found.", "instrument_not_found");

		var note = await noteRepository.FirstOrDefaultAsync(
			note => note.UserId == query.UserId && note.InstrumentId == query.InstrumentId,
			NoteResult.Projection,
			ct
		);
		if (note == null)
			throw new NotFoundException("Note not found.", "note_not_found");

		return note;
	}
}
