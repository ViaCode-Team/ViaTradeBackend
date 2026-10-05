using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Common.Abstractions.Repositories;
using ViaTrade.Application.Common.Exceptions;
using ViaTrade.Application.Instruments.Common;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Instruments.GetFile;

public sealed class GetInstrumentFileHandler(
	IFileReader tradefileReader,
	IReadRepository<Instrument> instrumentRepository
) : IQueryHandler<GetInstrumentFileQuery, InstrumentFileResult>
{
	public async Task<InstrumentFileResult> HandleAsync(GetInstrumentFileQuery query, CancellationToken ct = default)
	{
		string? symbol = null;
		int? instrumentId = null;

		bool isId = int.TryParse(query.InstrumentIdOrSymbol, out var parsedInstrumentId);
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
			symbol = query.InstrumentIdOrSymbol;
			instrumentId = await instrumentRepository.FirstOrDefaultAsync(
				instrument => instrument.Symbol == symbol,
				instrument => (int?)instrument.Id,
				ct
			);
		}

		var instrumentFiles = tradefileReader.GetInstruments(query.DataType, [symbol]);
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

		return new InstrumentFileResult
		{
			Id = instrumentId.Value,
			Symbol = instrumentFile.Symbol,
			TimeFrame = instrumentFile.TimeFrame,
			StartDate = instrumentFile.StartDate,
			EndDate = instrumentFile.EndDate,
		};
	}
}
