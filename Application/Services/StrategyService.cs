using Application.Contracts.Dto.Requests.Trade;
using Application.Models.Statistic;
using Application.Interfaces;
using Application.Interfaces.Repositories.Database;
using Application.Specifications;
using Domain.Entities.DataBase;
using Domain.Models.Filters;
using Domain.Models.Pagination;
using Domain.Models.Sort;

namespace Application.Services;

public class StrategyService(
	ITradeStrategyRepository tradeStrategyRepository,
	IUserTradeStrategyRepository userTradeStrategyRepository,
	IUserStrategyTradeCodeRepository userStrategyTradeCodeRepository) : IStrategyService
{
	private readonly ITradeStrategyRepository _tradeStrategyRepository = tradeStrategyRepository;
	private readonly IUserTradeStrategyRepository _userTradeStrategyRepository = userTradeStrategyRepository;
	private readonly IUserStrategyTradeCodeRepository _userStrategyTradeCodeRepository = userStrategyTradeCodeRepository;

	public async Task<PagedResult<TradeStrategy>> GetStrategiesPagedAsync(int userId, StrategyFilterRequest? filterRequest, StrategySortRequest? sortRequest, PaginationRequest? paginationRequest, CancellationToken cancellationToken)
	{
		var spec = new StrategyQuerySpecification(userId, filterRequest, sortRequest);
		return await _tradeStrategyRepository.GetPagedFilteredAsync(userId, spec, paginationRequest, cancellationToken);
	}

	public async Task<StrategyStatisticReadModel> GetStrategyStatisticAsync(int userId, CancellationToken cancellationToken)
	{
		var totalStrategiesTask = _tradeStrategyRepository.CountAsync(cancellationToken);
		var activeStrategiesTask = _userTradeStrategyRepository.CountByUserAsync(userId, cancellationToken);

		await Task.WhenAll(totalStrategiesTask, activeStrategiesTask);

		var totalStrategies = totalStrategiesTask.Result;
		var activeStrategies = activeStrategiesTask.Result;

		return new StrategyStatisticReadModel
		{
			TotalStrategies = totalStrategies,
			ActiveStrategies = activeStrategies,
			DisabledStrategies = Math.Max(totalStrategies - activeStrategies, 0)
		};
	}

	public async Task<TradeStrategy> GetStrategyByIdAsync(int strategyId, CancellationToken cancellationToken)
	{
		return await _tradeStrategyRepository.GetByIdAsync(strategyId, cancellationToken)
			?? throw new KeyNotFoundException();
	}

	public async Task<PagedResult<UserStrategyTradeCode>> GetUserStrategyCodesPagedAsync(int userId, PaginationRequest paginationRequest, CancellationToken cancellationToken)
	{
		return await _userStrategyTradeCodeRepository.GetPagedAsync(userId, paginationRequest, cancellationToken);
	}

	public async Task CreateUserStrategyCodeAsync(UserStrategyTradeCodeCreateDto request, int userId, CancellationToken cancellationToken)
	{
		bool isUserStrategyCodeExist = await _userStrategyTradeCodeRepository.ExistsAsync(
			e => e.UserId == userId &&
			e.StrategyId == request.StrategyId &&
			e.TradeCodeId == request.TradeCodeId,
			cancellationToken);

		if (isUserStrategyCodeExist)
			throw new InvalidOperationException("User strategy code already exists");

		var newUserStrategyCode = new UserStrategyTradeCode
		{
			StrategyId = request.StrategyId,
			TradeCodeId = request.TradeCodeId,
			UserId = userId
		};

		await _userStrategyTradeCodeRepository.AddAsync(newUserStrategyCode, cancellationToken);
		await _userStrategyTradeCodeRepository.SaveChangesAsync(cancellationToken);
	}

	public async Task DeleteUserStrategyCodeAsync(int strategyId, int tradeCodeId, int userId, CancellationToken cancellationToken)
	{
		var affectedRows = await _userStrategyTradeCodeRepository.ExecuteDeleteAsync(
			e => e.UserId == userId &&
				 e.StrategyId == strategyId &&
				 e.TradeCodeId == tradeCodeId,
			cancellationToken);

		if (affectedRows == 0)
			throw new KeyNotFoundException("User strategy code not found");
	}

	public async Task<PagedResult<UserTradeStrategy>> GetUserStrategiesPagedAsync(int userId, PaginationRequest paginationRequest, CancellationToken cancellationToken)
	{
		return await _userTradeStrategyRepository.GetByUserPagedAsync(userId, paginationRequest, cancellationToken);
	}

	public async Task CreateUserStrategyAsync(CreateUserStrategyCreateDto request, int userId, CancellationToken cancellationToken)
	{
		var isUserExist = await _userTradeStrategyRepository.ExistsAsync(e => e.UserId == userId && e.TradeStrategyId == request.StrategyId, cancellationToken);

		if (isUserExist)
			throw new InvalidOperationException("User strategy already exists");

		var strategyLink = new UserTradeStrategy
		{
			TradeStrategyId = request.StrategyId,
			UserId = userId
		};

		await _userTradeStrategyRepository.AddAsync(strategyLink, cancellationToken);
		await _userTradeStrategyRepository.SaveChangesAsync(cancellationToken);
	}

	public async Task DeleteUserStrategyAsync(int strategyId, int userId, CancellationToken cancellationToken)
	{
		var affectedRows = await _userTradeStrategyRepository.ExecuteDeleteAsync(
			e => e.UserId == userId && e.TradeStrategyId == strategyId,
			cancellationToken);

		if (affectedRows == 0)
			throw new KeyNotFoundException("User strategy not found");
	}
}
