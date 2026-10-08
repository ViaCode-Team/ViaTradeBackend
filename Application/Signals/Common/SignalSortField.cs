using System.Text.Json.Serialization;

namespace ViaTrade.Application.Signals.Common;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SignalSortField
{
	[JsonStringEnumMemberName("signalDateAsc")]
	SignalDateAsc,

	[JsonStringEnumMemberName("signalDateDesc")]
	SignalDateDesc,

	[JsonStringEnumMemberName("tickerAsc")]
	TickerAsc,

	[JsonStringEnumMemberName("tickerDesc")]
	TickerDesc,

	[JsonStringEnumMemberName("accuracyAsc")]
	AccuracyAsc,

	[JsonStringEnumMemberName("accuracyDesc")]
	AccuracyDesc,
}
