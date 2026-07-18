using Application.Common.Interfaces;
using Application.Common.Models.Filters;
using Application.Common.Models.Pagination;
using Application.Common.Specifications;
using Application.Notes.Interfaces;
using Domain.Notes.Entities;
using MediatR;

namespace Application.Notes.Queries;

public record GetUserNotePagedQuery(int UserId, NoteFilterRequest? FilterRequest, PaginationRequest? PaginationRequest) : IQuery<PagedResult<Note>>;

public class GetUserNotePagedQueryHandler(INoteRepository noteRepository) : IRequestHandler<GetUserNotePagedQuery, PagedResult<Note>>
{
	public async Task<PagedResult<Note>> Handle(GetUserNotePagedQuery request, CancellationToken cancellationToken)
	{
		var spec = new NoteQuerySpecification(request.UserId, request.FilterRequest);

		var pagination = request.PaginationRequest ?? new PaginationRequest();

		return await noteRepository.GetPagedAsync(spec, pagination, cancellationToken);
	}
}
