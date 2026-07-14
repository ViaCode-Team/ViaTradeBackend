using Application.Interfaces;
using Application.Interfaces.Repositories.Database;
using Domain.Entities.DataBase;
using Domain.Models.Dto;
using Domain.Models.Dto.Statistic;
using Domain.Models.Pagination;

namespace Application.Services;

public class TradeRemindService(
	ITradeRemindRepository tradeRemindRepository,
	ITradeCodeRepository tradeCodeRepository,
	IUserService userService) : ITradeRemindService
{
	private readonly ITradeRemindRepository _tradeRemindRepository = tradeRemindRepository;
	private readonly ITradeCodeRepository _tradeCodeRepository = tradeCodeRepository;
	private readonly IUserService _userService = userService;

	public async Task<IEnumerable<TradeRemind>> GetActualRemindAsync(CancellationToken cancellationToken)
	{
		return await _tradeRemindRepository.GetActualTradeRemind(cancellationToken);
	}

	public async Task<TradeRemindStatistic> GetTradeRemindStatisticAsync(int userId, CancellationToken cancellationToken)
	{
		await _userService.EnsureUserAsync(userId, cancellationToken);

		return new TradeRemindStatistic
		{
			TotalReminds = await _tradeRemindRepository.CountByUserAsync(userId, cancellationToken)
		};
	}

	public async Task DeleteActualRemindAsync(int remindId, CancellationToken cancellationToken)
	{
		await _tradeRemindRepository.ExecuteDeleteAsync(r => r.Id == remindId, cancellationToken);
	}

	public async Task<PagedResult<TradeRemind>> GetByUserPagedAsync(int userId, PaginationRequest paginationRequest, CancellationToken cancellationToken)
	{
		await _userService.EnsureUserAsync(userId, cancellationToken);
		return await _tradeRemindRepository.GetByUserPagedAsync(userId, paginationRequest, cancellationToken);
	}

	public async Task<PagedResult<TradeRemind>> GetByUserAndTradeCodePagedAsync(int userId, int tradeCodeId, PaginationRequest paginationRequest, CancellationToken cancellationToken)
	{
		await _userService.EnsureUserAsync(userId, cancellationToken);
		var tradeCode = await _tradeCodeRepository.GetByIdAsync(tradeCodeId, cancellationToken);
		if (tradeCode == null)
			throw new KeyNotFoundException();

		return await _tradeRemindRepository.GetByUserAndTradeCodePagedAsync(userId, tradeCodeId, paginationRequest, cancellationToken);
	}

	public async Task<TradeRemind> GetByIdAsync(int remindId, int userId, CancellationToken cancellationToken)
	{
		await _userService.EnsureUserAsync(userId, cancellationToken);
		var reminder = await _tradeRemindRepository.GetByIdAsync(remindId, cancellationToken);
		if (reminder == null || reminder.UserId != userId)
			throw new KeyNotFoundException();

		return reminder;
	}

	public async Task CreateAsync(int userId, int tradeCodeId, TradeRemindRequest request, CancellationToken cancellationToken)
	{
		await _userService.EnsureUserAsync(userId, cancellationToken);
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
		await _userService.EnsureUserAsync(userId, cancellationToken);
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
		await _userService.EnsureUserAsync(userId, cancellationToken);
		var remind = await _tradeRemindRepository.GetByIdAsync(remindId, cancellationToken);
		if (remind == null || remind.UserId != userId)
			throw new KeyNotFoundException();

		_tradeRemindRepository.Remove(remind);
		await _tradeRemindRepository.SaveChangesAsync(cancellationToken);
	}
}
