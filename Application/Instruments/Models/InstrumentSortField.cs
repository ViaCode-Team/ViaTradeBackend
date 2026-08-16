using System.Text.Json.Serialization;

namespace ViaTrade.Application.Instruments.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum InstrumentSortField
{
	[JsonStringEnumMemberName("symbolAsc")]
	SymbolAsc,

	[JsonStringEnumMemberName("symbolDesc")]
	SymbolDesc,
}
