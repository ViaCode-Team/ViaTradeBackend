using ViaTrade.Application.Common.Interfaces.Repositories;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Instruments.Interfaces;

public interface IInstrumentRepository : IRepository<Instrument>
{
	Task<Instrument?> FindByTickerAsync(string ticker, CancellationToken ct = default);
	Task<int?> FindIdByTickerAsync(string ticker, CancellationToken ct = default);
	Task<string?> FindTickerByIdAsync(int instrumentId, CancellationToken ct = default);
	Task<Dictionary<string, int>> GetInstrumentIdByTickerAsync(CancellationToken ct = default);
}
