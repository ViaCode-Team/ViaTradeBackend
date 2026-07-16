using Application.Interfaces;
using Application.Interfaces.Repositories.Database;
using Application.Specifications;
using Domain.Entities.DataBase;
using Domain.Models.Dto.Statistic;
using Domain.Models.Dto.Strategy;
using Domain.Models.Filters;
using Domain.Models.Pagination;
using Domain.Models.Sort;
using ViaTradeBackend.Models.Trade;

namespace Application.Services;

public class StrategyService(
	ITradeStrategyRepository tradeStrategyRepository,
	IUserTradeStrategyRepository userTradeStrategyRepository,
	IUserStrategyTradeCodeRepository userStrategyTradeCodeRepository) : IStrategyService
{
	private readonly ITradeStrategyRepository _tradeStrategyRepository = tradeStrategyRepository;
	private readonly IUserTradeStrategyRepository _userTradeStrategyRepository = userTradeStrategyRepository;
	private readonly IUserStrategyTradeCodeRepository _userStrategyTradeCodeRepository = userStrategyTradeCodeRepository;


	public async Task<PagedResult<TradeStrategyDto>> GetStrategiesPagedAsync(int userId, StrategyFilterRequest? filterRequest, StrategySortRequest? sortRequest, PaginationRequest? paginationRequest, CancellationToken cancellationToken)
	{


		var spec = new StrategySpecification(userId, filterRequest, sortRequest);
		return await _tradeStrategyRepository.GetPagedFilteredAsync(userId, spec, paginationRequest, cancellationToken);
	}

	public async Task<StrategyStatistic> GetStrategyStatisticAsync(int userId, CancellationToken cancellationToken)
	{


		var totalStrategiesTask = _tradeStrategyRepository.CountAsync(cancellationToken);
		var activeStrategiesTask = _userTradeStrategyRepository.CountByUserAsync(userId, cancellationToken);

		await Task.WhenAll(totalStrategiesTask, activeStrategiesTask);

		var totalStrategies = totalStrategiesTask.Result;
		var activeStrategies = activeStrategiesTask.Result;

		return new StrategyStatistic
		{
			TotalStrategies = totalStrategies,
			ActiveStrategies = activeStrategies,
			DisabledStrategies = Math.Max(totalStrategies - activeStrategies, 0)
		};
	}

	public async Task<TradeStrategy> GetStrategyByIdAsync(int strategyId, CancellationToken cancellationToken)
	{
		var strategy = await _tradeStrategyRepository.GetByIdAsync(strategyId, cancellationToken)
			?? throw new KeyNotFoundException();
		return strategy;
	}

	public async Task<PagedResult<UserStrategyTradeCodeDto>> GetUserStrategyCodesPagedAsync(int userId, PaginationRequest paginationRequest, CancellationToken cancellationToken)
	{


		return await _userStrategyTradeCodeRepository.GetPagedAsync(userId, paginationRequest, cancellationToken);
	}

	public async Task CreateUserStrategyCodeAsync(UserStrategyTradeCodeRequest request, int userId, CancellationToken cancellationToken)
	{


		var existing = await _userStrategyTradeCodeRepository.FindAsync(
			e => e.UserId == userId &&
				 e.StrategyId == request.StrategyId &&
				 e.TradeCodeId == request.TradeCodeId,
			cancellationToken);

		if (existing.Any())
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

	public async Task<PagedResult<UserTradeStrategyDto>> GetUserStrategiesPagedAsync(int userId, PaginationRequest paginationRequest, CancellationToken cancellationToken)
	{


		return await _userTradeStrategyRepository.GetByUserPagedAsync(userId, paginationRequest, cancellationToken);
	}

	public async Task CreateUserStrategyAsync(CreateUserStrategyRequest request, int userId, CancellationToken cancellationToken)
	{


		var existing = await _userTradeStrategyRepository.FindAsync(
			e => e.UserId == userId && e.TradeStrategyId == request.StrategyId,
			cancellationToken);

		if (existing.Any())
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
