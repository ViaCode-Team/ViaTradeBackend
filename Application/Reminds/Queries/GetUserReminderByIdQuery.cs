using Application.Common.Interfaces;
using Application.Reminds.Interfaces;
using Domain.Reminds.Entities;
using MediatR;

namespace Application.Reminds.Queries;

public record GetUserReminderByIdQuery(int RemindId, int UserId) : IQuery<TradeRemind>;

public class GetUserReminderByIdQueryHandler(ITradeRemindRepository tradeRemindRepository) : IRequestHandler<GetUserReminderByIdQuery, TradeRemind>
{
	public async Task<TradeRemind> Handle(GetUserReminderByIdQuery request, CancellationToken cancellationToken)
	{
		var reminds = await tradeRemindRepository.FindAsync(x => x.Id == request.RemindId && x.UserId == request.UserId, cancellationToken);
		var remind = reminds.FirstOrDefault();

		if (remind == null)
		{
			throw new Exception("Remind not found.");
		}

		return remind;
	}
}
