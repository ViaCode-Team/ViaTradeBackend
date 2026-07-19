using Application.Common.Interfaces;
using Application.Common.Models.Pagination;
using Application.Strategies.Interfaces;
using Domain.Strategies.Entities;
using MediatR;

namespace Application.Strategies.Queries;

public record GetUserStrategyCodesPagedQuery(int UserId, PaginationRequest PaginationRequest) : IQuery<PagedResult<UserStrategyTradeCode>>;

public class GetUserStrategyCodesPagedQueryHandler(IUserStrategyTradeCodeRepository userStrategyTradeCodeRepository)
	: IRequestHandler<GetUserStrategyCodesPagedQuery, PagedResult<UserStrategyTradeCode>>
{
	public async Task<PagedResult<UserStrategyTradeCode>> Handle(GetUserStrategyCodesPagedQuery request, CancellationToken cancellationToken)
	{
		return await userStrategyTradeCodeRepository.GetPagedAsync(request.UserId, request.PaginationRequest, cancellationToken);
	}
}
