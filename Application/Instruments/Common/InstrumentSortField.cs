using System.Text.Json.Serialization;

namespace ViaTrade.Application.Instruments.Common;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum InstrumentSortField
{
	[JsonStringEnumMemberName("symbolAsc")]
	SymbolAsc,

	[JsonStringEnumMemberName("symbolDesc")]
	SymbolDesc,
}
