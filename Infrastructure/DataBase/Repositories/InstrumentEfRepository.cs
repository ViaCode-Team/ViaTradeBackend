using Microsoft.EntityFrameworkCore;
using ViaTrade.Application.Instruments.Interfaces;
using ViaTrade.Application.Instruments.Models;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Infrastructure.DataBase.Repositories;

public class InstrumentEfRepository(AppDbContext context) : BaseEfRepository<Instrument>(context), IInstrumentRepository
{
	public async Task<Instrument?> FindByTickerAsync(string ticker, CancellationToken ct)
	{
		return await _dbSet.Where(e => e.Symbol == ticker).FirstOrDefaultAsync(ct);
	}

	public async Task<int?> FindIdByTickerAsync(string ticker, CancellationToken ct)
	{
		return await _dbSet.Where(e => e.Symbol == ticker).Select(e => (int?)e.Id).FirstOrDefaultAsync(ct);
	}

	public async Task<string?> FindTickerByIdAsync(int instrumentId, CancellationToken ct)
	{
		return await _dbSet.Where(e => e.Id == instrumentId).Select(e => e.Symbol).FirstOrDefaultAsync(ct);
	}

	public async Task<Dictionary<string, int>> GetInstrumentIdByTickerAsync(CancellationToken ct)
	{
		return await _dbSet
			.Select(instrument => new InstrumentReferenceDto(instrument.Id, instrument.Symbol))
			.ToDictionaryAsync(
				instrument => instrument.Symbol,
				instrument => instrument.Id,
				StringComparer.OrdinalIgnoreCase,
				ct
			);
	}
}
