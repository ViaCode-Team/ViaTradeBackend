using Microsoft.Extensions.Options;
using ViaTrade.Application.Common.Exceptions;
using ViaTrade.Application.Common.Interfaces.Repositories;
using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Common.Queries;
using ViaTrade.Application.Notes.Models;
using ViaTrade.Application.Reminders.Interfaces;
using ViaTrade.Application.Reminders.Models;
using ViaTrade.Application.Reminders.Specifications;
using ViaTrade.Configuration.Options;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Reminders;

public class ReminderQueryService(
	IReadRepository<Instrument> instrumentRepository,
	IReadRepository<Reminder> reminderRepository,
	IOptions<ReminderLimitsSettings> reminderLimitsOptions
) : IReminderQueryService
{
	public async Task<ReminderStatisticsDto> GetStatisticsAsync(int userId, CancellationToken ct)
	{
		int total = await reminderRepository.CountAsync(reminder => reminder.UserId == userId, ct);
		int remaining = Math.Max(0, reminderLimitsOptions.Value.MaxRemindersPerUser - total);

		return new ReminderStatisticsDto(total, reminderLimitsOptions.Value.MaxRemindersPerUser, remaining);
	}

	public async Task<IReadOnlyList<ReminderDto>> ListDueBatchAsync(int limit, CancellationToken ct)
	{
		ArgumentOutOfRangeException.ThrowIfLessThan(limit, 1);

		var specification = new DueRemindersSpecification(limit, DateTime.UtcNow);
		return await reminderRepository.ListAsync(specification, ct);
	}

	public async Task<Reminder> GetAsync(int userId, int reminderId, CancellationToken ct)
	{
		var specification = new ReminderWithInstrumentSpecification(userId, reminderId);
		var reminder = await reminderRepository.FirstOrDefaultAsync(specification, ct);
		if (reminder == null)
			throw new NotFoundException("Reminder not found.", "reminder_not_found");

		return reminder;
	}

	public async Task<PageResult<ReminderDto>> GetPageAsync(
		int userId,
		int instrumentId,
		ReminderFilter reminderFilter,
		ReminderSearch reminderSearch,
		PageOptions pageOptions,
		ReminderSort reminderSort,
		CancellationToken ct
	)
	{
		var instrumentExists = await instrumentRepository.AnyAsync(instrument => instrument.Id == instrumentId, ct);

		if (!instrumentExists)
			throw new NotFoundException("Instrument not found.", "instrument_not_found");

		var specification = new RemindersPageSpecification(
			userId,
			reminderFilter,
			reminderSearch,
			pageOptions,
			reminderSort,
			instrumentId
		);
		return await PageQuery.ExecuteAsync(
			reminderRepository,
			specification,
			reminder => new ReminderDto(
				reminder.Id,
				reminder.Text,
				reminder.RemindAt,
				new InstrumentBriefDto(
					reminder.InstrumentId,
					reminder.Instrument!.Symbol,
					reminder.Instrument.Description
				),
				reminder.UserId,
				string.Empty,
				reminder.DeliveredAt
			),
			ct
		);
	}

	public async Task<PageResult<ReminderDto>> GetPageAsync(
		int userId,
		ReminderFilter reminderFilter,
		ReminderSearch reminderSearch,
		PageOptions pageOptions,
		ReminderSort reminderSort,
		CancellationToken ct
	)
	{
		var specification = new RemindersPageSpecification(
			userId,
			reminderFilter,
			reminderSearch,
			pageOptions,
			reminderSort
		);
		return await PageQuery.ExecuteAsync(
			reminderRepository,
			specification,
			reminder => new ReminderDto(
				reminder.Id,
				reminder.Text,
				reminder.RemindAt,
				new InstrumentBriefDto(
					reminder.InstrumentId,
					reminder.Instrument!.Symbol,
					reminder.Instrument.Description
				),
				reminder.UserId,
				string.Empty,
				reminder.DeliveredAt
			),
			ct
		);
	}
}
