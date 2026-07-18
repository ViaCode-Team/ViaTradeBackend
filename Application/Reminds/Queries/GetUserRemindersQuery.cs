using Application.Interfaces.Repositories.Database;
using Application.Specifications;
using Domain.Reminds.Entities;
using Domain.Models.Pagination;
using Domain.Models.Sort;
using MediatR;

namespace Application.Reminds.Queries;

public record GetUserRemindersQuery(int UserId, PaginationRequest PaginationRequest, RemindSortRequest? SortRequest) : IRequest<PagedResult<TradeRemind>>;

public class GetUserRemindersQueryHandler(ITradeRemindRepository repository) : IRequestHandler<GetUserRemindersQuery, PagedResult<TradeRemind>>
{
    public async Task<PagedResult<TradeRemind>> Handle(GetUserRemindersQuery request, CancellationToken cancellationToken)
    {
        var spec = new TradeRemindQuerySpecification(request.UserId, null, request.SortRequest);
        return await repository.GetPagedAsync(spec, request.PaginationRequest, cancellationToken);
    }
}
