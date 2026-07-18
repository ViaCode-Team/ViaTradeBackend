using Application.Interfaces.Repositories.Database;
using Domain.Notes.Entities;
using Domain.Models.Filters;
using Domain.Models.Pagination;
using MediatR;
using Application.Specifications;

namespace Application.Notes.Queries;

public record GetUserNotePagedQuery(int UserId, NoteFilterRequest? FilterRequest, PaginationRequest? PaginationRequest) : IRequest<PagedResult<Note>>;

public class GetUserNotePagedQueryHandler(INoteRepository noteRepository) : IRequestHandler<GetUserNotePagedQuery, PagedResult<Note>>
{
    public async Task<PagedResult<Note>> Handle(GetUserNotePagedQuery request, CancellationToken cancellationToken)
    {
        var spec = new NoteQuerySpecification(request.UserId, request.FilterRequest);
        
        var pagination = request.PaginationRequest ?? new PaginationRequest();
        
        return await noteRepository.GetPagedAsync(spec, pagination, cancellationToken);
    }
}
