using Application.Common.Interfaces;
using Application.Strategies.Interfaces;
using Domain.Strategies.Entities;
using MediatR;

namespace Application.Strategies.Commands;

public record CreateUserStrategyCommand(int UserId, int StrategyId) : ITransactionalCommand;

public class CreateUserStrategyCommandHandler(IUserTradeStrategyRepository userTradeStrategyRepository)
	: IRequestHandler<CreateUserStrategyCommand>
{
	public async Task Handle(CreateUserStrategyCommand request, CancellationToken ct)
	{
		var isUserExist = await userTradeStrategyRepository.ExistsAsync(
			e => e.UserId == request.UserId && e.TradeStrategyId == request.StrategyId,
			ct);

		if (isUserExist)
			throw new InvalidOperationException("User strategy already exists");

		var strategyLink = new UserTradeStrategy
		{
			UserId = request.UserId,
			TradeStrategyId = request.StrategyId
		};

		await userTradeStrategyRepository.AddAsync(strategyLink, ct);
	}
}
