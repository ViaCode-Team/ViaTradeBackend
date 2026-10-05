using System.Linq.Expressions;
using ViaTrade.Application.Common.Models;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Reminders.Common;

public record ReminderResult(
	int Id,
	string Text,
	DateTime RemindAt,
	InstrumentBriefResult? Instrument,
	int UserId,
	string TelegramId,
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
				reminder.Instrument!.Symbol,
				reminder.Instrument.Description
			),
			reminder.UserId,
			string.Empty,
			reminder.DeliveredAt
		);

	public static Expression<Func<Reminder, ReminderResult>> DeliveryProjection { get; } =
		reminder => new ReminderResult(
			reminder.Id,
			reminder.Text,
			reminder.RemindAt,
			new InstrumentBriefResult(
				reminder.Instrument!.Id,
				reminder.Instrument.Symbol,
				reminder.Instrument.Description
			),
			reminder.UserId,
			reminder.User!.TelegramId!,
			reminder.DeliveredAt
		);
}
