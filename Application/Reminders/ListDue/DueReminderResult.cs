using System.Linq.Expressions;
using System.Text.Json.Serialization;
using ViaTrade.Application.Common.Models;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Reminders.ListDue;

public sealed record DueReminderResult(
	int Id,
	string Text,
	DateTime RemindAt,
	InstrumentBriefResult? Instrument,
	int UserId,
	[property: JsonIgnore] string TelegramId
)
{
	public static Expression<Func<Reminder, DueReminderResult>> Projection { get; } =
		reminder => new DueReminderResult(
			reminder.Id,
			reminder.Text,
			reminder.RemindAt,
			new InstrumentBriefResult(
				reminder.InstrumentId,
				reminder.Instrument!.Symbol,
				reminder.Instrument.Description
			),
			reminder.UserId,
			reminder.User!.TelegramId!
		);
}
