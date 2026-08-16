using ViaTrade.Application.Common.Models;

namespace ViaTrade.Application.Reminders.Models;

public record ReminderSort() : Sort<ReminderSortField>
{
	protected override List<ReminderSortField> DefaultSortBy => [ReminderSortField.RemindAtDesc];
}
