using ViaTrade.Application.Common.Interfaces;
using ViaTrade.Application.Common.Interfaces.Repositories;
using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Reminders.Models;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Reminders.Interfaces;

public interface IReminderRepository : IRepository<Reminder>
{
	Task<IReadOnlyList<ReminderDto>> ListDueAsync(CancellationToken ct = default);
	Task<PageResult<ReminderProjectionDto>> GetPageWithInstrumentAsync(
		IQuerySpecification<Reminder> specification,
		PageOptions pageOptions,
		CancellationToken ct = default
	);
	Task<Reminder?> FindByUserAndIdAsync(int userId, int reminderId, CancellationToken ct = default);
	Task<int> CountByUserAsync(int userId, CancellationToken ct = default);
	Task<int> ExecuteUpdateForUserAsync(
		int userId,
		int reminderId,
		string text,
		DateTime remindAt,
		CancellationToken ct = default
	);
	Task<int> ExecuteMarkPublishedAsync(int reminderId, CancellationToken ct = default);
	Task<int> ExecuteMarkDeliveredForUserAsync(int userId, int reminderId, CancellationToken ct = default);
	Task<int> ExecuteDeleteDeliveredBeforeAsync(DateTime deliveredBefore, CancellationToken ct = default);
}
