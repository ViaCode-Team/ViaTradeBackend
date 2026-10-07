using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Common.Abstractions.Repositories;
using ViaTrade.Application.Instruments.Common;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Instruments.ListFiles;

public sealed class ListInstrumentFilesHandler(
	IFileReader tradefileReader,
	IReadRepository<Instrument> instrumentRepository
) : IQueryHandler<ListInstrumentFilesQuery, IReadOnlyList<InstrumentFileResult>>
{
	public async Task<IReadOnlyList<InstrumentFileResult>> HandleAsync(
		ListInstrumentFilesQuery query,
		CancellationToken ct
	)
	{
		var instrumentFiles = tradefileReader.GetInstruments(query.DataType);
		var instruments = await instrumentRepository.ListAsync(
			instrument => true,
			instrument => new InstrumentReference(instrument.Id, instrument.Ticker),
			ct
		);
		var instrumentIdByTicker = instruments.ToDictionary(
			instrument => instrument.Ticker,
			instrument => instrument.Id,
			StringComparer.OrdinalIgnoreCase
		);

		return instrumentFiles
			.Where(file => instrumentIdByTicker.ContainsKey(file.Ticker))
			.Select(file => new InstrumentFileResult
			{
				Id = instrumentIdByTicker[file.Ticker],
				Ticker = file.Ticker,
				TimeFrame = file.TimeFrame,
				StartDate = file.StartDate,
				EndDate = file.EndDate,
			})
			.ToList();
	}
}
