using System.Text.Json.Serialization;

namespace ViaTrade.Application.Reminders.Common;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ReminderSortField
{
	[JsonStringEnumMemberName("remindAtAsc")]
	RemindAtAsc,

	[JsonStringEnumMemberName("remindAtDesc")]
	RemindAtDesc,
}
