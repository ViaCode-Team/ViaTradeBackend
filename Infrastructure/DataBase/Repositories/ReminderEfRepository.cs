using Microsoft.EntityFrameworkCore;
using ViaTrade.Application.Reminders.Interfaces;

namespace ViaTrade.Infrastructure.DataBase.Repositories;

public class ReminderEfRepository(AppDbContext context) : IReminderRepository
{
	public async Task<int> ExecuteUpdateForUserAsync(
		int userId,
		int reminderId,
		string text,
		DateTime remindAt,
		CancellationToken ct
	)
	{
		return await context
			.Reminders.Where(r =>
				r.Id == reminderId && r.UserId == userId && r.PublishedAt == null && r.DeliveredAt == null
			)
			.ExecuteUpdateAsync(
				s =>
					s.SetProperty(r => r.Text, text)
						.SetProperty(r => r.RemindAt, remindAt)
						.SetProperty(r => r.PublishedAt, (DateTime?)null),
				ct
			);
	}

	public async Task<int> ExecuteMarkPublishedAsync(int userId, int reminderId, CancellationToken ct)
	{
		var publishedAt = DateTime.UtcNow;

		return await context
			.Reminders.Where(r =>
				r.Id == reminderId && r.UserId == userId && r.RemindAt <= publishedAt && r.PublishedAt == null
			)
			.ExecuteUpdateAsync(s => s.SetProperty(r => r.PublishedAt, publishedAt), ct);
	}

	public async Task<int> ExecuteMarkDeliveredForUserAsync(int userId, int reminderId, CancellationToken ct)
	{
		var deliveredAt = DateTime.UtcNow;

		return await context
			.Reminders.Where(r => r.Id == reminderId && r.UserId == userId && r.RemindAt <= deliveredAt)
			.ExecuteUpdateAsync(
				s =>
					s.SetProperty(r => r.PublishedAt, r => r.PublishedAt ?? deliveredAt)
						.SetProperty(r => r.DeliveredAt, r => r.DeliveredAt ?? deliveredAt),
				ct
			);
	}
}
