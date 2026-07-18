using Application.Common.Models.Pagination;
using Application.Common.Models.Sort;
using Application.TradeCodes.Interfaces;
using Domain.TradeCodes.Entities;
using MediatR;

namespace Application.TradeCodes.Queries;

public record GetCodesPagedQuery(
	PaginationRequest PaginationRequest,
	StockSortRequest? SortRequest) : IRequest<PagedResult<TradeCode>>;

public class GetCodesPagedQueryHandler(ITradeCodeRepository tradeCodeRepository)
	: IRequestHandler<GetCodesPagedQuery, PagedResult<TradeCode>>
{
	private readonly ITradeCodeRepository _tradeCodeRepository = tradeCodeRepository;

	public async Task<PagedResult<TradeCode>> Handle(GetCodesPagedQuery request, CancellationToken cancellationToken)
	{
		return await _tradeCodeRepository.GetCodesPagedAsync(request.PaginationRequest, request.SortRequest, cancellationToken);
	}
}
