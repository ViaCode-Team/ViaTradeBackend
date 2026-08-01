using System.Text.Json.Serialization;

namespace Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum NoteType
{
	[JsonStringEnumMemberName("instrumentNote")]
	InstrumentNote,

	[JsonStringEnumMemberName("strategyNote")]
	StrategyNote,
}
