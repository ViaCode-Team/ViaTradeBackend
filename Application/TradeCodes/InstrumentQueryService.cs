using Application.Common.Exceptions;
using Application.Common.Models;
using Application.TradeCodes.Interfaces;
using Application.TradeCodes.Models;
using Application.Trades.Interfaces;
using Application.Trades.Models;
using Domain.Enums;
using Domain.TradeCodes.Entities;

namespace Application.TradeCodes;

public class InstrumentQueryService(IFileReader tradefileReader, ITradeCodeRepository tradeCodeRepository)
	: IInstrumentQueryService
{
	public async Task<InstrumentStatisticsDto> GetStatisticsAsync(CancellationToken ct)
	{
		int totalInstruments = await tradeCodeRepository.CountAsync(ct);

		return new InstrumentStatisticsDto(totalInstruments);
	}

	public async Task<TradeCode> GetAsync(int instrumentId, CancellationToken ct)
	{
		return await tradeCodeRepository.FindByIdAsync(instrumentId, ct)
			?? throw new NotFoundException("Instrument not found.", "instrument_not_found");
	}

	public async Task<TradeCode> GetBySymbolAsync(string symbol, CancellationToken ct)
	{
		return await tradeCodeRepository.FindByTickerAsync(symbol, ct)
			?? throw new NotFoundException("Instrument not found.", "instrument_not_found");
	}

	public async Task<PageResult<TradeCode>> GetPageAsync(
		InstrumentFilter instrumentFilter,
		PageOptions pageOptions,
		InstrumentSort instrumentSort,
		CancellationToken ct
	)
	{
		return await tradeCodeRepository.GetPageAsync(instrumentFilter, pageOptions, instrumentSort, ct);
	}

	public async Task<IReadOnlyList<InstrumentFileDto>> ListFileMetadataAsync(
		TradeDataType dataType,
		CancellationToken ct
	)
	{
		var instrumentFiles = tradefileReader.GetInstruments(dataType);
		var instrumentIdBySymbol = await tradeCodeRepository.GetTradeCodeIdByTickerAsync(ct);

		return instrumentFiles
			.Where(file => instrumentIdBySymbol.ContainsKey(file.Symbol))
			.Select(file => new InstrumentFileDto
			{
				Id = instrumentIdBySymbol[file.Symbol],
				Symbol = file.Symbol,
				TimeFrame = file.TimeFrame,
				StartDate = file.StartDate,
				EndDate = file.EndDate,
			})
			.ToList();
	}

	public async Task<InstrumentFileDto> GetFileMetadataAsync(
		TradeDataType dataType,
		string instrumentIdOrSymbol,
		CancellationToken ct
	)
	{
		string symbol;
		int? instrumentId;

		if (int.TryParse(instrumentIdOrSymbol, out var parsedInstrumentId))
		{
			symbol =
				await tradeCodeRepository.FindTickerByIdAsync(parsedInstrumentId, ct)
				?? throw new NotFoundException("Instrument not found.", "instrument_not_found");

			instrumentId = parsedInstrumentId;
		}
		else
		{
			symbol = instrumentIdOrSymbol;
			instrumentId = await tradeCodeRepository.FindIdByTickerAsync(symbol, ct);
		}

		var instrumentFiles = tradefileReader.GetInstruments(dataType, [symbol]);
		var instrumentFile = instrumentFiles.FirstOrDefault();
		if (instrumentFile == null)
			throw new NotFoundException("Instrument file not found.", "instrument_file_not_found");

		instrumentId ??= await tradeCodeRepository.FindIdByTickerAsync(instrumentFile.Symbol, ct);
		if (instrumentId == null)
			throw new NotFoundException("Instrument not found.", "instrument_not_found");

		return new InstrumentFileDto
		{
			Id = instrumentId.Value,
			Symbol = instrumentFile.Symbol,
			TimeFrame = instrumentFile.TimeFrame,
			StartDate = instrumentFile.StartDate,
			EndDate = instrumentFile.EndDate,
		};
	}
}
