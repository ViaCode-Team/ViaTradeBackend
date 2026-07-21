using Application.Common.Models;

namespace Application.Reminders.Models;

public record ReminderSort() : Sort<ReminderSortField>
{
	protected override List<ReminderSortField> DefaultSortBy => [ReminderSortField.DateTimeDesc];
}
