using Application.Interfaces.Repositories.Database;
using Domain.Models.Pagination;
using Domain.Strategies.Entities;
using MediatR;

namespace Application.Strategies.Queries;

public record GetUserStrategyCodesPagedQuery(int UserId, PaginationRequest PaginationRequest) : IRequest<PagedResult<UserStrategyTradeCode>>;

public class GetUserStrategyCodesPagedQueryHandler(IUserStrategyTradeCodeRepository userStrategyTradeCodeRepository) 
	: IRequestHandler<GetUserStrategyCodesPagedQuery, PagedResult<UserStrategyTradeCode>>
{
	private readonly IUserStrategyTradeCodeRepository _userStrategyTradeCodeRepository = userStrategyTradeCodeRepository;

	public async Task<PagedResult<UserStrategyTradeCode>> Handle(GetUserStrategyCodesPagedQuery request, CancellationToken cancellationToken)
	{
		return await _userStrategyTradeCodeRepository.GetPagedAsync(request.UserId, request.PaginationRequest, cancellationToken);
	}
}
