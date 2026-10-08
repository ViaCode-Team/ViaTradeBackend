using Microsoft.EntityFrameworkCore;
using ViaTrade.Application.Notes.Common.Abstractions;
using ViaTrade.Application.Notes.GetStatistics;

namespace ViaTrade.Infrastructure.DataBase.Repositories;

public class NoteEfRepository(AppDbContext context) : INoteRepository
{
	public async Task<NoteStatisticsResult> GetStatisticsAsync(int userId, CancellationToken ct)
	{
		var statistics = await context
			.Notes.Where(note => note.UserId == userId)
			.GroupBy(_ => 1)
			.Select(group => new NoteStatisticsResult(
				group.Count(),
				group.Count(note => note.InstrumentId != null),
				group.Count(note => note.StrategyId != null)
			))
			.SingleOrDefaultAsync(ct);

		return statistics ?? new NoteStatisticsResult(0, 0, 0);
	}

	public Task<int> ExecuteUpdateInstrumentAsync(int userId, int instrumentId, string text, CancellationToken ct)
	{
		return context
			.Notes.Where(note => note.UserId == userId && note.InstrumentId == instrumentId)
			.ExecuteUpdateAsync(setters => setters.SetProperty(note => note.Text, text), ct);
	}

	public Task<int> ExecuteUpdateStrategyAsync(int userId, int strategyId, string text, CancellationToken ct)
	{
		return context
			.Notes.Where(note => note.UserId == userId && note.StrategyId == strategyId)
			.ExecuteUpdateAsync(setters => setters.SetProperty(note => note.Text, text), ct);
	}
}
