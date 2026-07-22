using System.Text.Json.Serialization;

namespace Application.TradeCodes.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TradeCodeSortField
{
	[JsonStringEnumMemberName("nameAsc")]
	NameAsc,

	[JsonStringEnumMemberName("nameDesc")]
	NameDesc,
}
