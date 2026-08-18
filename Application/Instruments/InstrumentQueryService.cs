using ViaTrade.Application.Common.Exceptions;
using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Instruments.Interfaces;
using ViaTrade.Application.Instruments.Models;
using ViaTrade.Application.Instruments.Specifications;
using ViaTrade.Application.Trades.Interfaces;
using ViaTrade.Application.Trades.Models;
using ViaTrade.Domain.Entities;
using ViaTrade.Domain.Enums;

namespace ViaTrade.Application.Instruments;

public class InstrumentQueryService(IFileReader tradefileReader, IInstrumentRepository instrumentRepository)
	: IInstrumentQueryService
{
	public async Task<InstrumentStatisticsDto> GetStatisticsAsync(CancellationToken ct)
	{
		int totalInstruments = await instrumentRepository.CountAsync(ct);

		return new InstrumentStatisticsDto(totalInstruments);
	}

	public async Task<Instrument> GetAsync(int instrumentId, CancellationToken ct)
	{
		return await instrumentRepository.FindByIdAsync(instrumentId, ct)
			?? throw new NotFoundException("Instrument not found.", "instrument_not_found");
	}

	public async Task<Instrument> GetBySymbolAsync(string symbol, CancellationToken ct)
	{
		return await instrumentRepository.FindByTickerAsync(symbol, ct)
			?? throw new NotFoundException("Instrument not found.", "instrument_not_found");
	}

	public async Task<PageResult<Instrument>> GetPageAsync(
		InstrumentFilter instrumentFilter,
		InstrumentSearch instrumentSearch,
		PageOptions pageOptions,
		InstrumentSort instrumentSort,
		CancellationToken ct
	)
	{
		var spec = new InstrumentQuerySpecification(instrumentFilter, instrumentSearch, instrumentSort);
		return await instrumentRepository.GetPageAsync(spec, pageOptions, ct);
	}

	public async Task<IReadOnlyList<InstrumentFileDto>> ListFileMetadataAsync(
		TradeDataType dataType,
		CancellationToken ct
	)
	{
		var instrumentFiles = tradefileReader.GetInstruments(dataType);
		var instrumentIdBySymbol = await instrumentRepository.GetInstrumentIdByTickerAsync(ct);

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
		var hasInstrumentId = int.TryParse(instrumentIdOrSymbol, out var parsedInstrumentId);

		if (hasInstrumentId)
		{
			symbol =
				await instrumentRepository.FindTickerByIdAsync(parsedInstrumentId, ct)
				?? throw new NotFoundException("Instrument not found.", "instrument_not_found");

			instrumentId = parsedInstrumentId;
		}
		else
		{
			symbol = instrumentIdOrSymbol;
			instrumentId = await instrumentRepository.FindIdByTickerAsync(symbol, ct);
		}

		var instrumentFiles = tradefileReader.GetInstruments(dataType, [symbol]);
		var instrumentFile = instrumentFiles.FirstOrDefault();
		if (instrumentFile == null)
			throw new NotFoundException("Instrument file not found.", "instrument_file_not_found");

		instrumentId ??= await instrumentRepository.FindIdByTickerAsync(instrumentFile.Symbol, ct);
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
