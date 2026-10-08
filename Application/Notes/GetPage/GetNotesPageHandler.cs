using Mediator;
using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Common.Abstractions.Repositories;
using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Notes.Common;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Notes.GetPage;

public sealed class GetNotesPageHandler(IUserContext userContext, IReadRepository<Note> noteRepository)
	: IQueryHandler<GetNotesPageQuery, PageResult<NoteResult>>
{
	public async ValueTask<PageResult<NoteResult>> Handle(GetNotesPageQuery query, CancellationToken ct)
	{
		var specification = new NotesPageSpecification(
			userContext.UserId,
			query.NoteFilter,
			query.NoteSearch,
			query.PageOptions
		);

		return await noteRepository.GetPageAsync(specification, NoteResult.Projection, ct);
	}
}
