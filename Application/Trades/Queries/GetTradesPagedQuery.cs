using Application.Interfaces.Repositories.Database;
using Application.Specifications;
using Domain.Models.Filters;
using Domain.Models.Pagination;
using Domain.Trades.Entities;
using MediatR;

namespace Application.Trades.Queries;

public record GetTradesPagedQuery(int UserId, TradeFilterRequest? FilterRequest, PaginationRequest? PaginationRequest) : IRequest<PagedResult<Trade>>;

public class GetTradesPagedQueryHandler(ITradeRepository tradeRepository) 
	: IRequestHandler<GetTradesPagedQuery, PagedResult<Trade>>
{
	private readonly ITradeRepository _tradeRepository = tradeRepository;

	public async Task<PagedResult<Trade>> Handle(GetTradesPagedQuery request, CancellationToken cancellationToken)
	{
		var spec = new TradeQuerySpecification(request.UserId, request.FilterRequest);
		return await _tradeRepository.GetPagedFilteredAsync(spec, request.PaginationRequest, cancellationToken);
	}
}
