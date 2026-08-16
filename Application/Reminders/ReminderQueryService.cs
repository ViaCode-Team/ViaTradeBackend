using Microsoft.Extensions.Options;
using ViaTrade.Application.Common.Exceptions;
using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Instruments.Interfaces;
using ViaTrade.Application.Notes.Models;
using ViaTrade.Application.Reminders.Interfaces;
using ViaTrade.Application.Reminders.Models;
using ViaTrade.Application.Reminders.Specifications;
using ViaTrade.Configuration.Options;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Reminders;

public class ReminderQueryService(
	IInstrumentRepository instrumentRepository,
	IReminderRepository reminderRepository,
	IOptions<ReminderLimitsSettings> reminderLimitsOptions
) : IReminderQueryService
{
	public async Task<ReminderStatisticsDto> GetStatisticsAsync(int userId, CancellationToken ct)
	{
		int total = await reminderRepository.CountByUserAsync(userId, ct);
		int remaining = Math.Max(0, reminderLimitsOptions.Value.MaxRemindersPerUser - total);

		return new ReminderStatisticsDto(total, reminderLimitsOptions.Value.MaxRemindersPerUser, remaining);
	}

	public async Task<IReadOnlyList<ReminderDto>> ListDueAsync(CancellationToken ct)
	{
		return await reminderRepository.ListDueAsync(ct);
	}

	public async Task<Reminder> GetAsync(int userId, int reminderId, CancellationToken ct)
	{
		var reminder = await reminderRepository.FindByUserAndIdAsync(userId, reminderId, ct);
		if (reminder == null)
			throw new NotFoundException("Reminder not found.", "reminder_not_found");

		return reminder;
	}

	public async Task<PageResult<ReminderDto>> GetPageAsync(
		int userId,
		int instrumentId,
		ReminderDeliveryStatus deliveryStatus,
		PageOptions pageOptions,
		ReminderSort reminderSort,
		CancellationToken ct
	)
	{
		var instrumentExists = await instrumentRepository.ExistsAsync(instrument => instrument.Id == instrumentId, ct);

		if (!instrumentExists)
			throw new NotFoundException("Instrument not found.", "instrument_not_found");

		var spec = new ReminderQuerySpecification(userId, instrumentId, deliveryStatus, reminderSort);
		var reminders = await reminderRepository.GetPageWithInstrumentAsync(spec, pageOptions, ct);

		return reminders.Map(ToDto);
	}

	public async Task<PageResult<ReminderDto>> GetPageAsync(
		int userId,
		ReminderDeliveryStatus deliveryStatus,
		PageOptions pageOptions,
		ReminderSort reminderSort,
		CancellationToken ct
	)
	{
		var spec = new ReminderQuerySpecification(userId, null, deliveryStatus, reminderSort);
		var reminders = await reminderRepository.GetPageWithInstrumentAsync(spec, pageOptions, ct);

		return reminders.Map(ToDto);
	}

	private static ReminderDto ToDto(ReminderProjectionDto source)
	{
		var instrument = new InstrumentBriefDto(source.InstrumentId, source.InstrumentTicker, source.InstrumentName);

		return new ReminderDto(
			source.Id,
			source.Text,
			source.RemindAt,
			instrument,
			source.UserId,
			string.Empty,
			source.DeliveredAt
		);
	}
}
