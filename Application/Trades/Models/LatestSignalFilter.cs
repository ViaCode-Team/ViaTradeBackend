using System.Text.Json.Serialization;

namespace Application.Trades.Models;

public sealed class LatestSignalFilter
{
	public SignalDirection? Direction { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SignalDirection
{
	BUY,

	SELL,
}
