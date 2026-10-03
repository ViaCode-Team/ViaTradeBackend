using Ardalis.Specification;
using ViaTrade.Application.Notes.Models;
using ViaTrade.Application.Reminders.Models;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Reminders.Specifications;

public sealed class DueRemindersSpecification : Specification<Reminder, ReminderDto>
{
	public DueRemindersSpecification(int limit, DateTime utcNow)
	{
		Query
			.Where(reminder =>
				reminder.RemindAt <= utcNow && reminder.PublishedAt == null && reminder.User!.TelegramId != null
			)
			.OrderBy(reminder => reminder.RemindAt)
			.ThenBy(reminder => reminder.Id);
		Query.Take(limit);
		Query.Select(reminder => new ReminderDto(
			reminder.Id,
			reminder.Text,
			reminder.RemindAt,
			new InstrumentBriefDto(
				reminder.Instrument!.Id,
				reminder.Instrument.Symbol,
				reminder.Instrument.Description
			),
			reminder.UserId,
			reminder.User!.TelegramId!,
			reminder.DeliveredAt
		));
	}
}
