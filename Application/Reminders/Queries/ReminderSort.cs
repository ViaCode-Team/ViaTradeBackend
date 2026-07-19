using Application.Common.Queries;

namespace Application.Reminders.Queries;

public record ReminderSort() : Sort<ReminderSortField>
{
	protected override List<ReminderSortField> DefaultSortBy => [ReminderSortField.DateTimeDesc];
}
