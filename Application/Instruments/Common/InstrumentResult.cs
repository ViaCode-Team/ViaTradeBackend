using System.Linq.Expressions;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Instruments.Common;

public sealed record InstrumentResult(int Id, string Symbol, string? Description)
{
	public static Expression<Func<Instrument, InstrumentResult>> Projection { get; } =
		instrument => new InstrumentResult(instrument.Id, instrument.Symbol, instrument.Description);

	public static Expression<Func<UserStrategyInstrument, InstrumentResult>> LinkProjection { get; } =
		link => new InstrumentResult(link.Instrument!.Id, link.Instrument.Symbol, link.Instrument.Description);
}
