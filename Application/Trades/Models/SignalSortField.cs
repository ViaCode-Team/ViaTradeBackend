using System.Text.Json.Serialization;

namespace ViaTrade.Application.Trades.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SignalSortField
{
	[JsonStringEnumMemberName("signalDateAsc")]
	SignalDateAsc,

	[JsonStringEnumMemberName("signalDateDesc")]
	SignalDateDesc,

	[JsonStringEnumMemberName("symbolAsc")]
	SymbolAsc,

	[JsonStringEnumMemberName("symbolDesc")]
	SymbolDesc,

	[JsonStringEnumMemberName("accuracyAsc")]
	AccuracyAsc,

	[JsonStringEnumMemberName("accuracyDesc")]
	AccuracyDesc,
}
