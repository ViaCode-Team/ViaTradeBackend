using Domain.Users.Entities;
using Application.Interfaces.Repositories.Database;
using MediatR;

namespace Application.Strategies.Commands;

public record DeleteUserStrategyCommand(int UserId, int StrategyId) : IRequest;

public class DeleteUserStrategyCommandHandler(IUserTradeStrategyRepository userTradeStrategyRepository) 
	: IRequestHandler<DeleteUserStrategyCommand>
{
	private readonly IUserTradeStrategyRepository _userTradeStrategyRepository = userTradeStrategyRepository;

	public async Task Handle(DeleteUserStrategyCommand request, CancellationToken cancellationToken)
	{
		var affectedRows = await _userTradeStrategyRepository.ExecuteDeleteAsync(
			e => e.UserId == request.UserId && e.TradeStrategyId == request.StrategyId,
			cancellationToken);

		if (affectedRows == 0)
			throw new KeyNotFoundException("User strategy not found");
	}
}
