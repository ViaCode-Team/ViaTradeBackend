using Application.Auth.Interfaces;
using Application.Common.Interfaces;
using Application.TradeCodes.Interfaces;
using Application.Trades.Models;
using Domain.Trades.Entities;
using MediatR;

namespace Application.TradeCodes.Queries;

public record GetSysAllCodesQuery(TradeDataType DataType) : IQuery<IEnumerable<TradeCodeFileDto>>;

public class GetSysAllCodesQueryHandler(
	IFileReader tradefileReader,
	ITradeCodeRepository tradeCodeRepository)
	: IRequestHandler<GetSysAllCodesQuery, IEnumerable<TradeCodeFileDto>>
{
	private readonly IFileReader _tradefileReader = tradefileReader;
	private readonly ITradeCodeRepository _tradeCodeRepository = tradeCodeRepository;

	public async Task<IEnumerable<TradeCodeFileDto>> Handle(GetSysAllCodesQuery request, CancellationToken cancellationToken)
	{
		var tradeFiles = _tradefileReader.GetTradeCodes(request.DataType);
		var tradeCodes = await _tradeCodeRepository.GetAllAsync(cancellationToken);

		var dbCodeMap = tradeCodes.ToDictionary(
			tradeCode => tradeCode.ExchangeId,
			tradeCode => tradeCode.Id,
			StringComparer.OrdinalIgnoreCase);

		return tradeFiles
			.Where(fileCode => dbCodeMap.ContainsKey(fileCode.TradeCode))
			.Select(fileCode => new TradeCodeFileDto
			{
				Id = dbCodeMap[fileCode.TradeCode],
				ExchangeId = fileCode.TradeCode,
				TimeFrame = fileCode.TimeFrame,
				StartDate = fileCode.StartDate,
				EndDate = fileCode.EndDate
			});
	}
}
