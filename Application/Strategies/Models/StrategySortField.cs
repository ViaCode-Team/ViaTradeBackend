using System.Text.Json.Serialization;

namespace ViaTrade.Application.Strategies.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum StrategySortField
{
	[JsonStringEnumMemberName("nameAsc")]
	NameAsc,

	[JsonStringEnumMemberName("nameDesc")]
	NameDesc,

	[JsonStringEnumMemberName("accuracyAsc")]
	AccuracyAsc,

	[JsonStringEnumMemberName("accuracyDesc")]
	AccuracyDesc,
}
