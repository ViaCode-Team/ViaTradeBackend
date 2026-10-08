namespace ViaTrade.Application.Instruments.Common;

public sealed record InstrumentFileResult
{
	public required int Id { get; init; }

	public required string Ticker { get; init; }

	public required string TimeFrame { get; init; }

	public required DateTime StartDate { get; init; }

	public required DateTime EndDate { get; init; }
}
