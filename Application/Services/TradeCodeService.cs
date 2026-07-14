using Application.Interfaces;
using Application.Interfaces.Repositories.Database;
using Application.Interfaces.Utils;
using Domain.Entities.CSV;
using Domain.Entities.DataBase;
using Domain.Models.Dto.Statistic;
using Domain.Models.Dto.Trade;
using Domain.Models.Pagination;

namespace Application.Services;

public class TradeCodeService(
	IFileReader tradefileReader,
	ITradeCodeRepository tradeCodeRepository) : ITradeCodeService
{
	private readonly IFileReader _tradefileReader = tradefileReader;
	private readonly ITradeCodeRepository _tradeCodeRepository = tradeCodeRepository;

	public async Task<PagedResult<TradeCode>> GetCodesPagedAsync(PaginationRequest paginationRequest, CancellationToken cancellationToken = default)
	{
		return await _tradeCodeRepository.GetPagedAsync(paginationRequest, cancellationToken);
	}

	public async Task<StockStatistic> GetStockStatisticAsync(CancellationToken cancellationToken = default)
	{
		return new StockStatistic
		{
			TotalStocks = await _tradeCodeRepository.CountAsync(cancellationToken)
		};
	}

	public async Task<IEnumerable<TradeCodeFileDto>> GetSysAllCodesAsync(
		TradeDataType dataType,
		CancellationToken cancellationToken = default)
	{
		var tradeFiles = _tradefileReader.GetTradeCodes(dataType);
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

	public async Task<TradeCodeFileDto> GetSysCodeByIdAsync(
		TradeDataType dataType,
		string tradeIdString,
		CancellationToken cancellationToken = default)
	{
		string exchangeId;
		int? dbId = null;

		if (int.TryParse(tradeIdString, out var tradeCodeId))
		{
			var dbEntity = await _tradeCodeRepository.GetByIdAsync(tradeCodeId, cancellationToken)
				?? throw new KeyNotFoundException($"TradeCode with Id {tradeCodeId} not found in database");

			exchangeId = dbEntity.ExchangeId;
			dbId = dbEntity.Id;
		}
		else
		{
			exchangeId = tradeIdString;

			var dbEntities = await _tradeCodeRepository.FindAsync(
				c => c.ExchangeId == exchangeId,
				cancellationToken);

			dbId = dbEntities.FirstOrDefault()?.Id;
		}

		var fileCodes = _tradefileReader.GetTradeCodes(dataType, [exchangeId]);
		var fileCode = fileCodes.FirstOrDefault()
			?? throw new KeyNotFoundException($"No file data found for trade code '{exchangeId}'");

		if (dbId == null)
		{
			var dbEntities = await _tradeCodeRepository.FindAsync(
				c => c.ExchangeId == fileCode.TradeCode,
				cancellationToken);
			dbId = dbEntities.FirstOrDefault()?.Id;
		}

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
