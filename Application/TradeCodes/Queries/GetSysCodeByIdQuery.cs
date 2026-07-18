using Application.Auth.Interfaces;
using Application.TradeCodes.Interfaces;
using Application.Trades.Models;
using Domain.Trades.Entities;
using MediatR;

namespace Application.TradeCodes.Queries;

public record GetSysCodeByIdQuery(TradeDataType DataType, string TradeIdString) : IRequest<TradeCodeFileDto>;

public class GetSysCodeByIdQueryHandler(
	IFileReader tradefileReader,
	ITradeCodeRepository tradeCodeRepository)
	: IRequestHandler<GetSysCodeByIdQuery, TradeCodeFileDto>
{
	private readonly IFileReader _tradefileReader = tradefileReader;
	private readonly ITradeCodeRepository _tradeCodeRepository = tradeCodeRepository;

	public async Task<TradeCodeFileDto> Handle(GetSysCodeByIdQuery request, CancellationToken cancellationToken)
	{
		string exchangeId;
		int? dbId = null;

		if (int.TryParse(request.TradeIdString, out var tradeCodeId))
		{
			var dbEntity = await _tradeCodeRepository.GetByIdAsync(tradeCodeId, cancellationToken)
				?? throw new KeyNotFoundException($"TradeCode with Id {tradeCodeId} not found in database");

			exchangeId = dbEntity.ExchangeId;
			dbId = dbEntity.Id;
		}
		else
		{
			exchangeId = request.TradeIdString;
			dbId = await _tradeCodeRepository.GetIdByExchangeIdAsync(exchangeId, cancellationToken);
		}

		var fileCodes = _tradefileReader.GetTradeCodes(request.DataType, [exchangeId]);
		var fileCode = fileCodes.FirstOrDefault()
			?? throw new KeyNotFoundException($"No file data found for trade code '{exchangeId}'");

		dbId ??= await _tradeCodeRepository.GetIdByExchangeIdAsync(fileCode.TradeCode, cancellationToken);

		if (dbId == null)
			throw new KeyNotFoundException($"TradeCode '{exchangeId}' is not registered in database");

		return new TradeCodeFileDto
		{
			Id = dbId.Value,
			ExchangeId = fileCode.TradeCode,
			TimeFrame = fileCode.TimeFrame,
			StartDate = fileCode.StartDate,
			EndDate = fileCode.EndDate
		};
	}
}
