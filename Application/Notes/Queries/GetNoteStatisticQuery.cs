using Application.Interfaces.Repositories.Database;
using Application.Models.Statistic;
using MediatR;

namespace Application.Notes.Queries;

public record GetNoteStatisticQuery(int UserId) : IRequest<NoteStatisticReadModel>;

public class GetNoteStatisticQueryHandler(INoteRepository noteRepository) : IRequestHandler<GetNoteStatisticQuery, NoteStatisticReadModel>
{
    public async Task<NoteStatisticReadModel> Handle(GetNoteStatisticQuery request, CancellationToken cancellationToken)
    {
        return await noteRepository.GetNoteStatisticAsync(request.UserId, cancellationToken);
    }
}
