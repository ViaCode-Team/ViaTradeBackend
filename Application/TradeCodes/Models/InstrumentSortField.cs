using System.Text.Json.Serialization;

namespace Application.TradeCodes.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum InstrumentSortField
{
	[JsonStringEnumMemberName("symbolAsc")]
	SymbolAsc,

	[JsonStringEnumMemberName("symbolDesc")]
	SymbolDesc,
}
