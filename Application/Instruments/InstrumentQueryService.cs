using ViaTrade.Application.Common.Exceptions;
using ViaTrade.Application.Common.Interfaces.Repositories;
using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Common.Queries;
using ViaTrade.Application.Instruments.Interfaces;
using ViaTrade.Application.Instruments.Models;
using ViaTrade.Application.Instruments.Specifications;
using ViaTrade.Application.Trades.Interfaces;
using ViaTrade.Application.Trades.Models;
using ViaTrade.Domain.Entities;
using ViaTrade.Domain.Enums;

namespace ViaTrade.Application.Instruments;

public class InstrumentQueryService(IFileReader tradefileReader, IReadRepository<Instrument> instrumentRepository)
	: IInstrumentQueryService
{
	public async Task<InstrumentStatisticsDto> GetStatisticsAsync(CancellationToken ct)
	{
		int totalInstruments = await instrumentRepository.CountAsync(ct);

		return new InstrumentStatisticsDto(totalInstruments);
	}

	public async Task<Instrument> GetAsync(int instrumentId, CancellationToken ct)
	{
		return await instrumentRepository.GetByIdAsync(instrumentId, ct)
			?? throw new NotFoundException("Instrument not found.", "instrument_not_found");
	}

	public async Task<Instrument> GetBySymbolAsync(string symbol, CancellationToken ct)
	{
		return await instrumentRepository.FirstOrDefaultAsync(e => e.Symbol == symbol, ct)
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
		var specification = new InstrumentsPageSpecification(
			instrumentFilter,
			instrumentSearch,
			pageOptions,
			instrumentSort
		);
		return await PageQuery.ExecuteAsync(instrumentRepository, specification, ct);
	}

	public async Task<IReadOnlyList<InstrumentFileDto>> ListFileMetadataAsync(
		TradeDataType dataType,
		CancellationToken ct
	)
	{
		var instrumentFiles = tradefileReader.GetInstruments(dataType);
		var instruments = await instrumentRepository.ListAsync(
			instrument => true,
			instrument => new InstrumentReferenceDto(instrument.Id, instrument.Symbol),
			ct
		);
		var instrumentIdBySymbol = instruments.ToDictionary(
			instrument => instrument.Symbol,
			instrument => instrument.Id,
			StringComparer.OrdinalIgnoreCase
		);

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
		string? symbol = null;
		int? instrumentId = null;

		bool isId = int.TryParse(instrumentIdOrSymbol, out var parsedInstrumentId);
		if (isId)
		{
			symbol = await instrumentRepository.FirstOrDefaultAsync(
				instrument => instrument.Id == parsedInstrumentId,
				instrument => instrument.Symbol,
				ct
			);
			if (symbol != null)
				instrumentId = parsedInstrumentId;
		}

		if (symbol == null)
		{
			symbol = instrumentIdOrSymbol;
			instrumentId = await instrumentRepository.FirstOrDefaultAsync(
				instrument => instrument.Symbol == symbol,
				instrument => (int?)instrument.Id,
				ct
			);
		}

		var instrumentFiles = tradefileReader.GetInstruments(dataType, [symbol]);
		var instrumentFile = instrumentFiles.FirstOrDefault();
		if (instrumentFile == null)
			throw new NotFoundException("Instrument file not found.", "instrument_file_not_found");

		instrumentId ??= await instrumentRepository.FirstOrDefaultAsync(
			instrument => instrument.Symbol == instrumentFile.Symbol,
			instrument => (int?)instrument.Id,
			ct
		);
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
