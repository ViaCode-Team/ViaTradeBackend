using Application.Common.Interfaces;
using Application.Strategies.Interfaces;
using MediatR;

namespace Application.Strategies.Commands;

public record DeleteUserStrategyCommand(int UserId, int StrategyId) : ICommand;

public class DeleteUserStrategyCommandHandler(IUserTradeStrategyRepository userTradeStrategyRepository)
	: IRequestHandler<DeleteUserStrategyCommand>
{
	public async Task Handle(DeleteUserStrategyCommand request, CancellationToken ct)
	{
		var affectedRows = await userTradeStrategyRepository.ExecuteDeleteAsync(
			e => e.UserId == request.UserId && e.TradeStrategyId == request.StrategyId,
			ct);

		if (affectedRows == 0)
			throw new KeyNotFoundException("User strategy not found");
	}
}
