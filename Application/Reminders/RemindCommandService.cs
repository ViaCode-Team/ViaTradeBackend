using Application.Common.Interfaces;
using Application.Reminds.Interfaces;
using Domain.Reminds.Entities;

namespace Application.Reminds;

public class RemindCommandService(
	ITradeRemindRepository remindRepository,
	IUnitOfWork UoW) : IRemindCommandService
{
	public async Task CreateAsync(
		int userId,
		int tradeCodeId,
		string textRemind,
		DateTime dateTime,
		CancellationToken ct)
	{
		var remind = new Reminder
		{
			TextRemind = textRemind,
			DateTime = dateTime,
			TradeCodeId = tradeCodeId,
			UserId = userId
		};

		await remindRepository.AddAsync(remind, ct);

		await UoW.SaveChangesAsync(ct);
	}

	public async Task UpdateAsync(
		int remindId,
		int userId,
		string textRemind,
		DateTime dateTime,
		CancellationToken ct)
	{
		int rows = await remindRepository.ExecuteUpdateUserRemindAsync(
			remindId,
			userId,
			textRemind,
			dateTime,
			ct);

		if (rows == 0)
			throw new Exception("Remind not found.");
	}

	public async Task DeleteAsync(int remindId, int userId, CancellationToken ct)
	{
		int rows = await remindRepository.ExecuteDeleteAsync(
			x => x.Id == remindId && x.UserId == userId,
			ct);

		if (rows == 0)
			throw new Exception("Remind not found.");
	}

	public async Task DeleteAsync(int remindId, CancellationToken ct)
	{
		int rows = await remindRepository.ExecuteDeleteAsync(
			x => x.Id == remindId,
			ct);

		if (rows == 0)
			throw new Exception("Remind not found.");
	}
}
