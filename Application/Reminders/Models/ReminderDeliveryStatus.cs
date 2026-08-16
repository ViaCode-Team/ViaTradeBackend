using System.Text.Json.Serialization;

namespace ViaTrade.Application.Reminders.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ReminderDeliveryStatus
{
	[JsonStringEnumMemberName("all")]
	All,

	[JsonStringEnumMemberName("undelivered")]
	Undelivered,

	[JsonStringEnumMemberName("delivered")]
	Delivered,
}
