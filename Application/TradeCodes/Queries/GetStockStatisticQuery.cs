using Application.Common.Interfaces;
using Application.Statistics.Models;
using Application.TradeCodes.Interfaces;
using MediatR;

namespace Application.TradeCodes.Queries;

public record GetStockStatisticQuery : IQuery<StockStatisticReadModel>;

public class GetStockStatisticQueryHandler(ITradeCodeRepository tradeCodeRepository)
	: IRequestHandler<GetStockStatisticQuery, StockStatisticReadModel>
{
	private readonly ITradeCodeRepository _tradeCodeRepository = tradeCodeRepository;

	public async Task<StockStatisticReadModel> Handle(GetStockStatisticQuery request, CancellationToken cancellationToken)
	{
		return new StockStatisticReadModel
		{
			TotalStocks = await _tradeCodeRepository.CountAsync(cancellationToken)
		};
	}
}
