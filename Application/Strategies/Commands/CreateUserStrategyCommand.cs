using Application.Common.Interfaces;
using Application.Strategies.Interfaces;
using Domain.Strategies.Entities;
using MediatR;

namespace Application.Strategies.Commands;

public record CreateUserStrategyCommand(int UserId, int StrategyId) : ICommand;

public class CreateUserStrategyCommandHandler(IUserTradeStrategyRepository userTradeStrategyRepository)
	: IRequestHandler<CreateUserStrategyCommand>
{
	private readonly IUserTradeStrategyRepository _userTradeStrategyRepository = userTradeStrategyRepository;

	public async Task Handle(CreateUserStrategyCommand request, CancellationToken cancellationToken)
	{
		var isUserExist = await _userTradeStrategyRepository.ExistsAsync(
			e => e.UserId == request.UserId && e.TradeStrategyId == request.StrategyId,
			cancellationToken);

		if (isUserExist)
			throw new InvalidOperationException("User strategy already exists");

		var strategyLink = new UserTradeStrategy(request.UserId, request.StrategyId);

		await _userTradeStrategyRepository.AddAsync(strategyLink, cancellationToken);
	}
}
