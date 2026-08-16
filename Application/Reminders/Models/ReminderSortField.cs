using System.Text.Json.Serialization;

namespace ViaTrade.Application.Reminders.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ReminderSortField
{
	[JsonStringEnumMemberName("remindAtAsc")]
	RemindAtAsc,

	[JsonStringEnumMemberName("remindAtDesc")]
	RemindAtDesc,
}
