using Application.Interfaces.Repositories.Database;
using Domain.Reminds.Entities;
using MediatR;


namespace Application.Reminds.Queries;

public record GetActualRemindQuery() : IRequest<List<TradeRemind>>;

public class GetActualRemindQueryHandler(ITradeRemindRepository repository) : IRequestHandler<GetActualRemindQuery, List<TradeRemind>>
{
    public async Task<List<TradeRemind>> Handle(GetActualRemindQuery request, CancellationToken cancellationToken)
    {
        var reminds = await repository.FindAsync(x => x.DateTime <= DateTime.UtcNow, cancellationToken);
        return reminds.ToList();
    }
}
