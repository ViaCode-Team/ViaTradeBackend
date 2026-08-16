using System.Text.Json.Serialization;

namespace ViaTrade.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TradeStatus
{
	[JsonStringEnumMemberName("open")]
	Open,

	[JsonStringEnumMemberName("closed")]
	Closed,
}
