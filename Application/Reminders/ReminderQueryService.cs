using Application.Common.Models.Pagination;
using Application.Common.Models.Sort;
using Application.Common.Specifications;
using Application.Reminds.Interfaces;
using Application.Reminds.Models;
using Domain.Reminds.Entities;

namespace Application.Reminds;

public class ReminderQueryService(ITradeRemindRepository remindRepository) : IRemindQueryService
{
	public async Task<RemindStatisticDto> GetStatistics(int userId, CancellationToken ct)
	{
		int total = await remindRepository.CountAsync(
			x => x.UserId == userId, ct);

		return new RemindStatisticDto(total);
	}

	public async Task<IEnumerable<Reminder>> GetActualAsync(CancellationToken ct)
	{
		return await remindRepository.FindAsync(
			x => x.DateTime <= DateTime.UtcNow, ct);
	}

	public async Task<Reminder> GetAsync(int remindId, int userId, CancellationToken ct)
	{
		return await remindRepository.FirstOrDefaultAsync(
			x => x.Id == remindId && x.UserId == userId, ct)
			?? throw new Exception("Remind not found.");
	}

	public async Task<PagedResult<Reminder>> GetPagedAsync(
		int userId,
		int tradeCodeId,
		PaginationRequest paginationRequest,
		RemindSortRequest? sortRequest,
		CancellationToken ct)
	{
		var spec = new TradeRemindQuerySpecification(userId, tradeCodeId, sortRequest);
		return await remindRepository.GetPagedAsync(spec, paginationRequest, ct);
	}

	public async Task<PagedResult<Reminder>> GetPagedAsync(
		int userId,
		PaginationRequest paginationRequest,
		RemindSortRequest? sortRequest,
		CancellationToken ct)
	{
		var spec = new TradeRemindQuerySpecification(userId, null, sortRequest);
		return await remindRepository.GetPagedAsync(spec, paginationRequest, ct);
	}
}
