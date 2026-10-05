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
		CancellationToken ct = default
	)
	{
		var instrumentFiles = tradefileReader.GetInstruments(query.DataType);
		var instruments = await instrumentRepository.ListAsync(
			instrument => true,
			instrument => new InstrumentReference(instrument.Id, instrument.Symbol),
			ct
		);
		var instrumentIdBySymbol = instruments.ToDictionary(
			instrument => instrument.Symbol,
			instrument => instrument.Id,
			StringComparer.OrdinalIgnoreCase
		);

		return instrumentFiles
			.Where(file => instrumentIdBySymbol.ContainsKey(file.Symbol))
			.Select(file => new InstrumentFileResult
			{
				Id = instrumentIdBySymbol[file.Symbol],
				Symbol = file.Symbol,
				TimeFrame = file.TimeFrame,
				StartDate = file.StartDate,
				EndDate = file.EndDate,
			})
			.ToList();
	}
}
