using Domain.Users.Entities;
using Application.Interfaces.Repositories.Database;
using MediatR;

namespace Application.Strategies.Commands;

public record CreateUserStrategyCommand(int UserId, int StrategyId) : IRequest;

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

		var strategyLink = new Domain.Strategies.Entities.UserTradeStrategy(request.UserId, request.StrategyId);

		await _userTradeStrategyRepository.AddAsync(strategyLink, cancellationToken);
		await _userTradeStrategyRepository.SaveChangesAsync(cancellationToken);
	}
}
