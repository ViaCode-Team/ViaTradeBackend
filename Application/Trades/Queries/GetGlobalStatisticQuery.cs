using Application.Common.Interfaces;
using Application.Statistics.Models;
using Application.Trades.Interfaces;
using MediatR;

namespace Application.Trades.Queries;

public record GetGlobalStatisticQuery(int UserId) : IQuery<GlobalStatisticReadModel>;

public class GetGlobalStatisticQueryHandler(ITradeRepository tradeRepository)
	: IRequestHandler<GetGlobalStatisticQuery, GlobalStatisticReadModel>
{
	private readonly ITradeRepository _tradeRepository = tradeRepository;

	public async Task<GlobalStatisticReadModel> Handle(GetGlobalStatisticQuery request, CancellationToken cancellationToken)
	{
		return await _tradeRepository.GetGlobalStatisticAsync(request.UserId, cancellationToken);
	}
}
