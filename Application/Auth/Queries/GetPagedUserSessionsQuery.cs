using Application.Contracts.Dto.User;
using Application.Interfaces.Repositories.Redis;
using Domain.Models.Pagination;
using MediatR;

namespace Application.Auth.Queries;

public record GetPagedUserSessionsQuery(int UserId, PaginationRequest PaginationRequest) : IRequest<PagedResult<UserSessionDto>>;

public class GetPagedUserSessionsQueryHandler(ISessionRepository sessionRepository) 
	: IRequestHandler<GetPagedUserSessionsQuery, PagedResult<UserSessionDto>>
{
	private readonly ISessionRepository _sessionRepository = sessionRepository;

	public async Task<PagedResult<UserSessionDto>> Handle(GetPagedUserSessionsQuery request, CancellationToken cancellationToken)
	{
		return await _sessionRepository.GetPagedUserSessionsAsync(request.UserId, request.PaginationRequest);
	}
}
