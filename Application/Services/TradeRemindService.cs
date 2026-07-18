using Application.Contracts.Dto.NoteRemind;
using Application.Contracts.Dto.Statistic;
using Application.Interfaces;
using Application.Interfaces.Repositories.Database;
using Domain.Entities.DataBase;
using Domain.Models.Pagination;
using Domain.Models.Sort;

using Application.Contracts.Dto.Requests.Remind;

namespace Application.Services;
public class TradeRemindService(
	ITradeRemindRepository tradeRemindRepository,
	ITradeCodeRepository tradeCodeRepository) : ITradeRemindService
{
	private readonly ITradeRemindRepository _tradeRemindRepository = tradeRemindRepository;
	private readonly ITradeCodeRepository _tradeCodeRepository = tradeCodeRepository;

	public async Task<IEnumerable<TradeRemind>> GetActualRemindAsync(CancellationToken cancellationToken)
	{
		return await _tradeRemindRepository.GetActualRemind(cancellationToken);
	}

	public async Task<TradeRemindStatisticDto> GetRemindStatisticAsync(int userId, CancellationToken cancellationToken)
	{

		return new TradeRemindStatisticDto
		{
			TotalReminds = await _tradeRemindRepository.CountByUserAsync(userId, cancellationToken)
		};
	}

	public async Task DeleteActualRemindAsync(int remindId, CancellationToken cancellationToken)
	{
		await _tradeRemindRepository.ExecuteDeleteAsync(r => r.Id == remindId, cancellationToken);
	}

	public async Task<PagedResult<TradeRemindDto>> GetByUserPagedAsync(int userId, PaginationRequest paginationRequest, RemindSortRequest? sortRequest = null, CancellationToken cancellationToken = default)
	{

		return await _tradeRemindRepository.GetByUserPagedAsync(userId, paginationRequest, sortRequest, cancellationToken);
	}

	public async Task<PagedResult<TradeRemindDto>> GetByUserAndTradeCodePagedAsync(int userId, int tradeCodeId, PaginationRequest paginationRequest, RemindSortRequest? sortRequest = null, CancellationToken cancellationToken = default)
	{

		var tradeCode = await _tradeCodeRepository.GetByIdAsync(tradeCodeId, cancellationToken);
		if (tradeCode == null)
			throw new KeyNotFoundException($"TradeCode with id: {tradeCodeId} not found");

		return await _tradeRemindRepository.GetByUserAndTradeCodePagedAsync(userId, tradeCodeId, paginationRequest, sortRequest, cancellationToken);
	}

	public async Task<TradeRemind> GetByIdAsync(int remindId, int userId, CancellationToken cancellationToken)
	{
		var reminder = await _tradeRemindRepository.GetByIdAsync(remindId, cancellationToken);
		if (reminder == null || reminder.UserId != userId)
			throw new KeyNotFoundException();

		return reminder;
	}

	public async Task CreateAsync(int userId, int tradeCodeId, TradeRemindCreateDto request, CancellationToken cancellationToken)
	{
		bool isTradeCodeExist = await _tradeCodeRepository.ExistsAsync(c => c.Id == tradeCodeId, cancellationToken);
		if (!isTradeCodeExist)
			throw new KeyNotFoundException();

		var remind = new TradeRemind
		{
			TextRemind = request.TextRemind,
			DateTime = request.DateTime,
			TradeCodeId = tradeCodeId,
			UserId = userId
		};

		await _tradeRemindRepository.AddAsync(remind, cancellationToken);
		await _tradeRemindRepository.SaveChangesAsync(cancellationToken);
	}

	public async Task UpdateAsync(int remindId, int userId, TradeRemindCreateDto request, CancellationToken cancellationToken)
	{
		var affectedRows = await _tradeRemindRepository.UpdateUserRemindAsync(remindId, userId, request.TextRemind, request.DateTime, cancellationToken);
		if (affectedRows == 0)
			throw new KeyNotFoundException();
	}

	public async Task DeleteAsync(int remindId, int userId, CancellationToken cancellationToken)
	{
		var affectedRows = await _tradeRemindRepository.ExecuteDeleteAsync(r => r.Id == remindId && r.UserId == userId, cancellationToken);
		if (affectedRows == 0)
		{
			bool exists = await _tradeRemindRepository.ExistsAsync(r => r.Id == remindId, cancellationToken);
			if (exists)
				throw new UnauthorizedAccessException();
			throw new KeyNotFoundException();
		}
	}
}
