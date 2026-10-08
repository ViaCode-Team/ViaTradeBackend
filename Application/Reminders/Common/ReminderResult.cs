using System.Linq.Expressions;
using ViaTrade.Application.Common.Models;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Reminders.Common;

public sealed record ReminderResult(
	int Id,
	string Text,
	DateTime RemindAt,
	InstrumentBriefResult? Instrument,
	DateTime? DeliveredAt
)
{
	public static Expression<Func<Reminder, ReminderResult>> Projection { get; } =
		reminder => new ReminderResult(
			reminder.Id,
			reminder.Text,
			reminder.RemindAt,
			new InstrumentBriefResult(
				reminder.InstrumentId,
				reminder.Instrument!.Ticker,
				reminder.Instrument.Description
			),
			reminder.DeliveredAt
		);
}
