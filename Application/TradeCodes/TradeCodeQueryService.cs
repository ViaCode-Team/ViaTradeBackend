using Application.Common.Models;
using Application.TradeCodes.Interfaces;
using Application.TradeCodes.Models;
using Application.Trades.Interfaces;
using Application.Trades.Models;
using Domain.Enums;
using Domain.TradeCodes.Entities;

namespace Application.TradeCodes;

public class TradeCodeQueryService(IFileReader tradefileReader, ITradeCodeRepository tradeCodeRepository)
	: ITradeCodeQueryService
{
	public async Task<StockStatisticDto> GetStatisticsAsync(CancellationToken ct)
	{
		int totalStocksCount = await tradeCodeRepository.CountAsync(ct);

		return new StockStatisticDto(totalStocksCount);
	}

	public async Task<PageResult<TradeCode>> GetPageAsync(
		PageOptions pageOptions,
		TradeCodeSort tradeCodeSort,
		CancellationToken ct
	)
	{
		return await tradeCodeRepository.GetPageAsync(pageOptions, tradeCodeSort, ct);
	}

	public async Task<IReadOnlyList<TradeCodeFileDto>> ListFileMetadataAsync(
		TradeDataType dataType,
		CancellationToken ct
	)
	{
		var tradeFiles = tradefileReader.GetTradeCodes(dataType);
		var dbCodeMap = await tradeCodeRepository.GetExchangeIdMapAsync(ct);

		return tradeFiles
			.Where(fileCode => dbCodeMap.ContainsKey(fileCode.TradeCode))
			.Select(fileCode => new TradeCodeFileDto
			{
				Id = dbCodeMap[fileCode.TradeCode],
				ExchangeId = fileCode.TradeCode,
				TimeFrame = fileCode.TimeFrame,
				StartDate = fileCode.StartDate,
				EndDate = fileCode.EndDate,
			})
			.ToList();
	}

	public async Task<TradeCodeFileDto> GetFileMetadataAsync(
		TradeDataType dataType,
		string tradeIdString,
		CancellationToken ct
	)
	{
		string exchangeId;
		int? dbId;

		if (int.TryParse(tradeIdString, out var tradeCodeId))
		{
			exchangeId =
				await tradeCodeRepository.FindExchangeIdByIdAsync(tradeCodeId, ct)
				?? throw new KeyNotFoundException($"TradeCode with Id {tradeCodeId} not found in database");

			dbId = tradeCodeId;
		}
		else
		{
			exchangeId = tradeIdString;
			dbId = await tradeCodeRepository.FindIdByExchangeIdAsync(exchangeId, ct);
		}

		var fileCodes = tradefileReader.GetTradeCodes(dataType, [exchangeId]);
		var fileCode = fileCodes.FirstOrDefault();
		if (fileCode == null)
			throw new KeyNotFoundException($"No file data found for trade code '{exchangeId}'");

		dbId ??= await tradeCodeRepository.FindIdByExchangeIdAsync(fileCode.TradeCode, ct);
		if (dbId == null)
			throw new KeyNotFoundException($"TradeCode '{exchangeId}' is not registered in database");

		return new TradeCodeFileDto
		{
			Id = dbId.Value,
			ExchangeId = fileCode.TradeCode,
			TimeFrame = fileCode.TimeFrame,
			StartDate = fileCode.StartDate,
			EndDate = fileCode.EndDate,
		};
	}
}
