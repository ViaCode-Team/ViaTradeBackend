using System.Linq.Expressions;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Common.Models;

public sealed record InstrumentBriefResult(int Id, string Symbol, string? Name)
{
	public static Expression<Func<Instrument, InstrumentBriefResult>> Projection { get; } =
		instrument => new InstrumentBriefResult(instrument.Id, instrument.Symbol, instrument.Description);
}
