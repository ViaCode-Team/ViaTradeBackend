using Application.Interfaces;
using Application.Interfaces.Repositories.Database;
using Domain.Entities.DataBase;
using Domain.Models.Dto;
using Domain.Models.Dto.NoteRemind;
using Domain.Models.Dto.Statistic;
using Domain.Models.Pagination;
using Domain.Models.Sort;

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

	public async Task<TradeRemindStatistic> GetRemindStatisticAsync(int userId, CancellationToken cancellationToken)
	{


		return new TradeRemindStatistic
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

		if (tradeCode is null)
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

	public async Task CreateAsync(int userId, int tradeCodeId, TradeRemindRequest request, CancellationToken cancellationToken)
	{


		var tradeCode = await _tradeCodeRepository.GetByIdAsync(tradeCodeId, cancellationToken);
		if (tradeCode == null)
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

	public async Task UpdateAsync(int remindId, int userId, TradeRemindRequest request, CancellationToken cancellationToken)
	{


		var remind = await _tradeRemindRepository.GetByIdAsync(remindId, cancellationToken);
		if (remind == null || remind.UserId != userId)
			throw new KeyNotFoundException();

		var tradeCode = await _tradeCodeRepository.GetByIdAsync(remind.TradeCodeId, cancellationToken);
		if (tradeCode == null)
			throw new KeyNotFoundException();

		remind.TextRemind = request.TextRemind;
		remind.DateTime = request.DateTime;

		_tradeRemindRepository.Update(remind);
		await _tradeRemindRepository.SaveChangesAsync(cancellationToken);
	}

	public async Task DeleteAsync(int remindId, int userId, CancellationToken cancellationToken)
	{


		var remind = await _tradeRemindRepository.GetByIdAsync(remindId, cancellationToken);
		if (remind == null || remind.UserId != userId)
			throw new KeyNotFoundException();

		_tradeRemindRepository.Remove(remind);
		await _tradeRemindRepository.SaveChangesAsync(cancellationToken);
	}
}
