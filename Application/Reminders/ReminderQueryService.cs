using Application.Common.Exceptions;
using Application.Common.Models;
using Application.Common.Specifications;
using Application.Notes.Models;
using Application.Reminders.Interfaces;
using Application.Reminders.Models;
using Domain.Reminders.Entities;

namespace Application.Reminders;

public class ReminderQueryService(IReminderRepository reminderRepository) : IReminderQueryService
{
	public async Task<ReminderStatisticsDto> GetStatisticsAsync(int userId, CancellationToken ct)
	{
		int total = await reminderRepository.CountAsync(x => x.UserId == userId, ct);

		return new ReminderStatisticsDto(total);
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
		int tradeCodeId,
		PageOptions pageOptions,
		ReminderSort reminderSort,
		CancellationToken ct
	)
	{
		var spec = new ReminderQuerySpecification(userId, tradeCodeId, reminderSort);
		var reminders = await reminderRepository.GetPageWithTradeCodeAsync(spec, pageOptions, ct);

		return reminders.Map(ToDto);
	}

	public async Task<PageResult<ReminderDto>> GetPageAsync(
		int userId,
		PageOptions pageOptions,
		ReminderSort reminderSort,
		CancellationToken ct
	)
	{
		var spec = new ReminderQuerySpecification(userId, null, reminderSort);
		var reminders = await reminderRepository.GetPageWithTradeCodeAsync(spec, pageOptions, ct);

		return reminders.Map(ToDto);
	}

	private static ReminderDto ToDto(ReminderProjectionDto source)
	{
		var instrument = new InstrumentBriefDto(source.TradeCodeId, source.TradeCodeTicker, source.TradeCodeName);

		return new ReminderDto(source.Id, source.Text, source.RemindAt, instrument, source.UserId);
	}
}
