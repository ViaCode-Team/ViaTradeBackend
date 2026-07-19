using Application.Common.Interfaces;
using Application.Reminds.Interfaces;
using MediatR;

namespace Application.Reminds.Queries;

public record TradeRemindStatisticReadModel(int TotalReminds);

public record GetRemindStatisticQuery(int UserId) : IQuery<TradeRemindStatisticReadModel>;

public class GetRemindStatisticQueryHandler(ITradeRemindRepository tradeRemindRepository) : IRequestHandler<GetRemindStatisticQuery, TradeRemindStatisticReadModel>
{
	public async Task<TradeRemindStatisticReadModel> Handle(GetRemindStatisticQuery request, CancellationToken cancellationToken)
	{
		var total = await tradeRemindRepository.CountAsync(x => x.UserId == request.UserId, cancellationToken);
		return new TradeRemindStatisticReadModel(total);
	}
}
