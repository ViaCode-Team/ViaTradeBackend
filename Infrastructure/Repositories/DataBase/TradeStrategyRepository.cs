using Application.Interfaces.Repositories.Database;
using Domain.Entities.DataBase;
using Domain.Interfaces;
using Domain.Models.Dto.Strategy;
using Domain.Models.Pagination;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.DataBase;

public class TradeStrategyRepository(AppDbContext context) : GenericRepository<TradeStrategy, TradeStrategyDto>(context),
	ITradeStrategyRepository
{
	public async Task<int> CountAsync(CancellationToken cancellationToken = default)
	{
		return await _dbSet.CountAsync(cancellationToken);
	}

	public async Task<TradeStrategyDto?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
	{
		return await _dbSet
			.Where(tradeStrategy => tradeStrategy.Name == name)
			.Select(tradeStrategy => new TradeStrategyDto
			{
				Id = tradeStrategy.Id,
				Name = tradeStrategy.Name,
				Description = tradeStrategy.Description,
				Accuracy = tradeStrategy.Accuracy,
				SignalFrequency = tradeStrategy.SignalFrequency,
				InvestmentHorizon = tradeStrategy.InvestmentHorizon,
				LogicDesc = tradeStrategy.LogicDesc,
				UseDesc = tradeStrategy.UseDesc,
				LimitDesc = tradeStrategy.LimitDesc
			})
			.FirstOrDefaultAsync(cancellationToken);
	}

	public async Task<PagedResult<TradeStrategyDto>> GetPagedFilteredAsync(int userId, ISpecification<TradeStrategy> spec, PaginationRequest? paginationRequest, CancellationToken cancellationToken = default)
	{
		var queryable = SpecificationEvaluator.GetQuery(_dbSet.AsQueryable(), spec);

		return await queryable
			.Select(tradeStrategy => new TradeStrategyDto
			{
				Id = tradeStrategy.Id,
				Name = tradeStrategy.Name,
				Description = tradeStrategy.Description,
				Accuracy = tradeStrategy.Accuracy,
				SignalFrequency = tradeStrategy.SignalFrequency,
				InvestmentHorizon = tradeStrategy.InvestmentHorizon,
				LogicDesc = tradeStrategy.LogicDesc,
				UseDesc = tradeStrategy.UseDesc,
				LimitDesc = tradeStrategy.LimitDesc,
				IsActive = tradeStrategy.UserTradeStrategies != null && tradeStrategy.UserTradeStrategies.Any(uts => uts.UserId == userId)
			})
			.ToPagedAsync(paginationRequest, cancellationToken);
	}
}
