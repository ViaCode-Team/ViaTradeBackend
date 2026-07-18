using Application.Interfaces.Repositories.Database;
using Application.Specifications;
using Domain.Models.Filters;
using Domain.Models.Pagination;
using Domain.Models.Sort;
using Domain.Strategies.Entities;
using MediatR;

namespace Application.Strategies.Queries;

public record GetStrategiesPagedQuery(int UserId, StrategyFilterRequest? FilterRequest, StrategySortRequest? SortRequest, PaginationRequest? PaginationRequest) : IRequest<PagedResult<TradeStrategy>>;

public class GetStrategiesPagedQueryHandler(ITradeStrategyRepository tradeStrategyRepository) 
	: IRequestHandler<GetStrategiesPagedQuery, PagedResult<TradeStrategy>>
{
	private readonly ITradeStrategyRepository _tradeStrategyRepository = tradeStrategyRepository;

	public async Task<PagedResult<TradeStrategy>> Handle(GetStrategiesPagedQuery request, CancellationToken cancellationToken)
	{
		var spec = new StrategyQuerySpecification(request.UserId, request.FilterRequest, request.SortRequest);
		return await _tradeStrategyRepository.GetPagedFilteredAsync(request.UserId, spec, request.PaginationRequest, cancellationToken);
	}
}
