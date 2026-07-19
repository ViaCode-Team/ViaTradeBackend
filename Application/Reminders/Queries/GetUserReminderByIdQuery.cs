using Application.Common.Interfaces;
using Application.Reminds.Interfaces;
using Domain.Reminds.Entities;
using MediatR;

namespace Application.Reminds.Queries;

public record GetUserReminderByIdQuery(int RemindId, int UserId) : IQuery<Reminder>;

public class GetUserReminderByIdQueryHandler(ITradeRemindRepository tradeRemindRepository) : IRequestHandler<GetUserReminderByIdQuery, Reminder>
{
	public async Task<Reminder> Handle(GetUserReminderByIdQuery request, CancellationToken ct)
	{
		var reminds = await tradeRemindRepository.FindAsync(x => x.Id == request.RemindId && x.UserId == request.UserId, ct);
		var remind = reminds.FirstOrDefault();

		if (remind == null)
		{
			throw new Exception("Remind not found.");
		}

		return remind;
	}
}
