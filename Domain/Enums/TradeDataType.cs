using System.Text.Json.Serialization;

namespace ViaTrade.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TradeDataType
{
	[JsonStringEnumMemberName("futures")]
	Futures,

	[JsonStringEnumMemberName("stocks")]
	Stocks,

	[JsonStringEnumMemberName("strategy")]
	Strategy,

	[JsonStringEnumMemberName("screener")]
	Screener,
}
