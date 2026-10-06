using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Common.Abstractions.Repositories;
using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Notes.Common;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Notes.GetPage;

public sealed class GetNotesPageHandler(IReadRepository<Note> noteRepository)
	: IQueryHandler<GetNotesPageQuery, PageResult<NoteResult>>
{
	public async Task<PageResult<NoteResult>> HandleAsync(GetNotesPageQuery query, CancellationToken ct = default)
	{
		var specification = new NotesPageSpecification(
			query.UserId,
			query.NoteFilter,
			query.NoteSearch,
			query.PageOptions
		);
		return await noteRepository.GetPageAsync(specification, NoteResult.Projection, ct);
	}
}
