using Mediator;
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
	public async ValueTask<InstrumentFileResult> Handle(GetInstrumentFileQuery query, CancellationToken ct)
	{
		string? ticker = null;
		int? instrumentId = null;

		bool isId = int.TryParse(query.InstrumentIdOrTicker, out var parsedInstrumentId);
		if (isId)
		{
			ticker = await instrumentRepository.FirstOrDefaultAsync(
				instrument => instrument.Id == parsedInstrumentId,
				instrument => instrument.Ticker,
				ct
			);
			if (ticker != null)
				instrumentId = parsedInstrumentId;
		}

		if (ticker == null)
		{
			ticker = query.InstrumentIdOrTicker;
			instrumentId = await instrumentRepository.FirstOrDefaultAsync(
				instrument => instrument.Ticker == ticker,
				instrument => (int?)instrument.Id,
				ct
			);
		}

		var instrumentFiles = tradefileReader.GetInstruments(query.DataType, [ticker]);
		var instrumentFile = instrumentFiles.FirstOrDefault();
		if (instrumentFile == null)
			throw new NotFoundException("Instrument file not found.", "instrument_file_not_found");

		instrumentId ??= await instrumentRepository.FirstOrDefaultAsync(
			instrument => instrument.Ticker == instrumentFile.Ticker,
			instrument => (int?)instrument.Id,
			ct
		);
		if (instrumentId == null)
			throw new NotFoundException("Instrument not found.", "instrument_not_found");

		return new InstrumentFileResult
		{
			Id = instrumentId.Value,
			Ticker = instrumentFile.Ticker,
			TimeFrame = instrumentFile.TimeFrame,
			StartDate = instrumentFile.StartDate,
			EndDate = instrumentFile.EndDate,
		};
	}
}
