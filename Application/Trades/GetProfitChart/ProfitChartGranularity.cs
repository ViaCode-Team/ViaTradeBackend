using System.Text.Json.Serialization;

namespace ViaTrade.Application.Trades.GetProfitChart;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ProfitChartGranularity
{
	[JsonStringEnumMemberName("day")]
	Day,

	[JsonStringEnumMemberName("week")]
	Week,

	[JsonStringEnumMemberName("month")]
	Month,
}
