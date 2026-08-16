using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using Application.Reminders.Models;
using Domain.Entities;

namespace Application.Reminders.Interfaces;

public interface IReminderRepository : IRepository<Reminder>
{
	Task<IReadOnlyList<ReminderDto>> ListDueAsync(CancellationToken ct = default);
	Task<PageResult<ReminderProjectionDto>> GetPageWithInstrumentAsync(
		IQuerySpecification<Reminder> specification,
		PageOptions pageOptions,
		CancellationToken ct = default
	);
	Task<PageResult<ReminderProjectionDto>> GetPageSearchAsync(
		ISearchSpecification<Reminder> specification,
		PageOptions pageOptions,
		CancellationToken ct
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
	Task<int> ExecuteMarkDeliveredForUserAsync(
		int userId,
		int reminderId,
		CancellationToken ct = default
	);
	Task<int> ExecuteDeleteDeliveredBeforeAsync(DateTime deliveredBefore, CancellationToken ct = default);
}
