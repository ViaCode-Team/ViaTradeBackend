using Application.Common.Interfaces;
using Application.Reminds.Interfaces;
using Domain.Reminds.Entities;
using MediatR;

namespace Application.Reminds.Queries;

public record GetActualRemindQuery() : IQuery<List<Reminder>>;

public class GetActualRemindQueryHandler(ITradeRemindRepository tradeRemindRepository) : IRequestHandler<GetActualRemindQuery, List<Reminder>>
{
	public async Task<List<Reminder>> Handle(GetActualRemindQuery request, CancellationToken ct)
	{
		var reminds = await tradeRemindRepository.FindAsync(x => x.DateTime <= DateTime.UtcNow, ct);
		return reminds.ToList();
	}
}
