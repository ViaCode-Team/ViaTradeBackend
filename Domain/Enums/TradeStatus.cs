using System.Text.Json.Serialization;

namespace Domain.Trades.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TradeStatus
{
	[JsonStringEnumMemberName("open")]
	Open,

	[JsonStringEnumMemberName("closed")]
	Closed,
}
