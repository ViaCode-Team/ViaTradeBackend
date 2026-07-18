using Application.Interfaces.Repositories.Database;
using Application.Models.Statistic;
using MediatR;

namespace Application.Trades.Queries;

public record GetGlobalStatisticQuery(int UserId) : IRequest<GlobalStatisticReadModel>;

public class GetGlobalStatisticQueryHandler(ITradeRepository tradeRepository) 
	: IRequestHandler<GetGlobalStatisticQuery, GlobalStatisticReadModel>
{
	private readonly ITradeRepository _tradeRepository = tradeRepository;

	public async Task<GlobalStatisticReadModel> Handle(GetGlobalStatisticQuery request, CancellationToken cancellationToken)
	{
		return await _tradeRepository.GetGlobalStatisticAsync(request.UserId, cancellationToken);
	}
}
