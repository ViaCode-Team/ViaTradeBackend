using Application.Common.Interfaces;
using Application.Common.Models.Pagination;
using Application.Common.Models.Sort;
using Application.Common.Specifications;
using Application.Reminds.Interfaces;
using Domain.Reminds.Entities;
using MediatR;

namespace Application.Reminds.Queries;

public record GetUserRemindersByInstrumentQuery(int UserId, int TradeCodeId, PaginationRequest PaginationRequest, RemindSortRequest? SortRequest) : IQuery<PagedResult<Reminder>>;

public class GetUserRemindersByInstrumentQueryHandler(ITradeRemindRepository tradeRemindRepository) : IRequestHandler<GetUserRemindersByInstrumentQuery, PagedResult<Reminder>>
{
	public async Task<PagedResult<Reminder>> Handle(GetUserRemindersByInstrumentQuery request, CancellationToken ct)
	{
		var spec = new TradeRemindQuerySpecification(request.UserId, request.TradeCodeId, request.SortRequest);
		return await tradeRemindRepository.GetPagedAsync(spec, request.PaginationRequest, ct);
	}
}
