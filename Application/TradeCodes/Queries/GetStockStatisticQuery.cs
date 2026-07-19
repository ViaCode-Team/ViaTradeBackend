using Application.Common.Interfaces;
using Application.Statistics.Models;
using Application.TradeCodes.Interfaces;
using MediatR;

namespace Application.TradeCodes.Queries;

public record GetStockStatisticQuery : IQuery<StockStatisticReadModel>;

public class GetStockStatisticQueryHandler(ITradeCodeRepository tradeCodeRepository)
	: IRequestHandler<GetStockStatisticQuery, StockStatisticReadModel>
{
	public async Task<StockStatisticReadModel> Handle(GetStockStatisticQuery request, CancellationToken ct)
	{
		return new StockStatisticReadModel
		{
			TotalStocks = await tradeCodeRepository.CountAsync(ct)
		};
	}
}
