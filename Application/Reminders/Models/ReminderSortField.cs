using System.Text.Json.Serialization;

namespace Application.Reminders.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ReminderSortField
{
	[JsonStringEnumMemberName("dateTimeAsc")]
	DateTimeAsc,

	[JsonStringEnumMemberName("dateTimeDesc")]
	DateTimeDesc,
}
