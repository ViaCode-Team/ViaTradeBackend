using Application.Common.Interfaces;
using Application.Common.Models.Filters;
using Application.Common.Models.Pagination;
using Application.Common.Specifications;
using Application.Trades.Interfaces;
using Domain.Trades.Entities;
using MediatR;

namespace Application.Trades.Queries;

public record GetTradesPagedQuery(int UserId, TradeFilterRequest? FilterRequest, PaginationRequest? PaginationRequest) : IQuery<PagedResult<Trade>>;

public class GetTradesPagedQueryHandler(ITradeRepository tradeRepository)
	: IRequestHandler<GetTradesPagedQuery, PagedResult<Trade>>
{
	public async Task<PagedResult<Trade>> Handle(GetTradesPagedQuery request, CancellationToken ct)
	{
		var spec = new TradeQuerySpecification(request.UserId, request.FilterRequest);
		return await tradeRepository.GetPagedFilteredAsync(spec, request.PaginationRequest, ct);
	}
}
