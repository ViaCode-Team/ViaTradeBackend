using Application.Auth.Interfaces;
using Application.Common.Models.Pagination;
using Application.Common.Models.Sort;
using Application.Statistics.Models;
using Application.TradeCodes.Interfaces;
using Application.Trades.Models;
using Domain.TradeCodes.Entities;
using Domain.Trades.Entities;

namespace Application.TradeCodes;

public class TradeCodeQueryService(
	IFileReader tradefileReader,
	ITradeCodeRepository tradeCodeRepository) : ITradeCodeQueryService
{
	public async Task<PagedResult<TradeCode>> GetAsync(PaginationRequest paginationRequest, StockSortRequest sortRequest, CancellationToken ct)
	{
		return await tradeCodeRepository.GetCodesPagedAsync(paginationRequest, sortRequest, ct);
	}

	public async Task<StockStatisticReadModel> GetStatisticsAsync(CancellationToken ct)
	{
		return new StockStatisticReadModel
		{
			TotalStocks = await tradeCodeRepository.CountAsync(ct)
		};
	}

	public async Task<IEnumerable<TradeCodeFileDto>> GetSystemAsync(TradeDataType dataType, CancellationToken ct)
	{
		var tradeFiles = tradefileReader.GetTradeCodes(dataType);
		var tradeCodes = await tradeCodeRepository.GetAllAsync(ct);

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

	public async Task<TradeCodeFileDto> GetSystemAsync(TradeDataType dataType, string tradeIdString, CancellationToken ct)
	{
		string exchangeId;
		int? dbId = null;

		if (int.TryParse(tradeIdString, out var tradeCodeId))
		{
			var dbEntity = await tradeCodeRepository.GetByIdAsync(tradeCodeId, ct)
				?? throw new KeyNotFoundException($"TradeCode with Id {tradeCodeId} not found in database");

			exchangeId = dbEntity.ExchangeId;
			dbId = dbEntity.Id;
		}
		else
		{
			exchangeId = tradeIdString;
			dbId = await tradeCodeRepository.GetIdByExchangeIdAsync(exchangeId, ct);
		}

		var fileCodes = tradefileReader.GetTradeCodes(dataType, [exchangeId]);
		var fileCode = fileCodes.FirstOrDefault()
			?? throw new KeyNotFoundException($"No file data found for trade code '{exchangeId}'");

		dbId ??= await tradeCodeRepository.GetIdByExchangeIdAsync(fileCode.TradeCode, ct);

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
