using Application.Auth.Interfaces;
using Application.Common.Interfaces;
using Application.Common.Models.Pagination;
using Application.Users.Models;
using MediatR;

namespace Application.Auth.Queries;

public record GetPagedUserSessionsQuery(int UserId, PaginationRequest PaginationRequest) : IQuery<PagedResult<UserSessionDto>>;

public class GetPagedUserSessionsQueryHandler(ISessionRepository sessionRepository)
	: IRequestHandler<GetPagedUserSessionsQuery, PagedResult<UserSessionDto>>
{
	public async Task<PagedResult<UserSessionDto>> Handle(GetPagedUserSessionsQuery request, CancellationToken ct)
	{
		return await sessionRepository.GetPagedUserSessionsAsync(request.UserId, request.PaginationRequest);
	}
}
