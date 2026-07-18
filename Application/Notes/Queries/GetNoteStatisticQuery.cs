using Application.Common.Interfaces;
using Application.Notes.Interfaces;
using Application.Statistics.Models;
using MediatR;

namespace Application.Notes.Queries;

public record GetNoteStatisticQuery(int UserId) : IQuery<NoteStatisticReadModel>;

public class GetNoteStatisticQueryHandler(INoteRepository noteRepository) : IRequestHandler<GetNoteStatisticQuery, NoteStatisticReadModel>
{
	public async Task<NoteStatisticReadModel> Handle(GetNoteStatisticQuery request, CancellationToken cancellationToken)
	{
		return await noteRepository.GetNoteStatisticAsync(request.UserId, cancellationToken);
	}
}
