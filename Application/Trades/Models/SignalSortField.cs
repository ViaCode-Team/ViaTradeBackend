using System.Text.Json.Serialization;

namespace Application.Trades.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SignalSortField
{
	[JsonStringEnumMemberName("dateTimeAsc")]
	DateTimeAsc,

	[JsonStringEnumMemberName("dateTimeDesc")]
	DateTimeDesc,

	[JsonStringEnumMemberName("assetAsc")]
	AssetAsc,

	[JsonStringEnumMemberName("assetDesc")]
	AssetDesc,

	[JsonStringEnumMemberName("accuracyAsc")]
	AccuracyAsc,

	[JsonStringEnumMemberName("accuracyDesc")]
	AccuracyDesc,
}
