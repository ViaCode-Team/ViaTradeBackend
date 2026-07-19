using Application.Common.Interfaces;
using Application.Common.Models.Pagination;
using Application.Common.Models.Sort;
using Application.Common.Specifications;
using Application.Reminds.Interfaces;
using Domain.Reminds.Entities;
using MediatR;

namespace Application.Reminds.Queries;

public record GetUserRemindersQuery(int UserId, PaginationRequest PaginationRequest, RemindSortRequest? SortRequest) : IQuery<PagedResult<TradeRemind>>;

public class GetUserRemindersQueryHandler(ITradeRemindRepository tradeRemindRepository) : IRequestHandler<GetUserRemindersQuery, PagedResult<TradeRemind>>
{
	public async Task<PagedResult<TradeRemind>> Handle(GetUserRemindersQuery request, CancellationToken cancellationToken)
	{
		var spec = new TradeRemindQuerySpecification(request.UserId, null, request.SortRequest);
		return await tradeRemindRepository.GetPagedAsync(spec, request.PaginationRequest, cancellationToken);
	}
}
