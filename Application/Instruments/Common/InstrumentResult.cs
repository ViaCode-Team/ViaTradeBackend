using System.Linq.Expressions;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Instruments.Common;

public sealed record InstrumentResult(int Id, string Ticker, string? Description)
{
	public static Expression<Func<Instrument, InstrumentResult>> Projection { get; } =
		instrument => new InstrumentResult(instrument.Id, instrument.Ticker, instrument.Description);

	public static Expression<Func<UserStrategyInstrument, InstrumentResult>> LinkProjection { get; } =
		link => new InstrumentResult(link.Instrument!.Id, link.Instrument.Ticker, link.Instrument.Description);
}
