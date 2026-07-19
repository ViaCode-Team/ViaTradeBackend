using Application.Common.Interfaces;
using Application.Common.Models.Pagination;
using Application.Strategies.Interfaces;
using Domain.Strategies.Entities;
using MediatR;

namespace Application.Strategies.Queries;

public record GetUserStrategiesPagedQuery(int UserId, PaginationRequest PaginationRequest) : IQuery<PagedResult<UserTradeStrategy>>;

public class GetUserStrategiesPagedQueryHandler(IUserTradeStrategyRepository userTradeStrategyRepository)
	: IRequestHandler<GetUserStrategiesPagedQuery, PagedResult<UserTradeStrategy>>
{
	public async Task<PagedResult<UserTradeStrategy>> Handle(GetUserStrategiesPagedQuery request, CancellationToken cancellationToken)
	{
		return await userTradeStrategyRepository.GetByUserPagedAsync(request.UserId, request.PaginationRequest, cancellationToken);
	}
}
