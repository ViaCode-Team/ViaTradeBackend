using System.Text.Json.Serialization;

namespace Domain.Notes.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum NoteType
{
	[JsonStringEnumMemberName("tradeCodeNote")]
	TradeCodeNote,

	[JsonStringEnumMemberName("tradeStrategyNote")]
	TradeStrategyNote,
}
