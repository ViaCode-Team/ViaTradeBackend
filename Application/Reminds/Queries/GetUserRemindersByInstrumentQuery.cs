using Application.Interfaces.Repositories.Database;
using Application.Specifications;
using Domain.Reminds.Entities;
using Domain.Models.Pagination;
using Domain.Models.Sort;
using MediatR;

namespace Application.Reminds.Queries;

public record GetUserRemindersByInstrumentQuery(int UserId, int TradeCodeId, PaginationRequest PaginationRequest, RemindSortRequest? SortRequest) : IRequest<PagedResult<TradeRemind>>;

public class GetUserRemindersByInstrumentQueryHandler(ITradeRemindRepository repository) : IRequestHandler<GetUserRemindersByInstrumentQuery, PagedResult<TradeRemind>>
{
    public async Task<PagedResult<TradeRemind>> Handle(GetUserRemindersByInstrumentQuery request, CancellationToken cancellationToken)
    {
        var spec = new TradeRemindQuerySpecification(request.UserId, request.TradeCodeId, request.SortRequest);
        return await repository.GetPagedAsync(spec, request.PaginationRequest, cancellationToken);
    }
}
