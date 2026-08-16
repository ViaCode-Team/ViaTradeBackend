using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using Application.Instruments.Models;
using Application.Instruments.Specifications;
using Domain.Entities;

namespace Application.Instruments.Interfaces;

public interface IInstrumentRepository : IRepository<Instrument>
{
	Task<Instrument?> FindByTickerAsync(string ticker, CancellationToken ct = default);
	Task<int?> FindIdByTickerAsync(string ticker, CancellationToken ct = default);
	Task<string?> FindTickerByIdAsync(int instrumentId, CancellationToken ct = default);
	Task<Dictionary<string, int>> GetInstrumentIdByTickerAsync(CancellationToken ct = default);
	Task<PageResult<Instrument>> GetPageAsync(
		InstrumentFilter instrumentFilter,
		PageOptions pageOptions,
		InstrumentSort instrumentSort,
		CancellationToken ct = default
		);
	Task<PageResult<Instrument>> GetPageSearchAsync(
		InstrumentSearchSpecification specification,
		PageOptions pageOptions,
		CancellationToken ct = default
		);
}
