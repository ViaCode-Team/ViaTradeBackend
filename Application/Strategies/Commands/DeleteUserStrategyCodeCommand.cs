using Application.Common.Interfaces;
using Application.Strategies.Interfaces;
using MediatR;

namespace Application.Strategies.Commands;

public record DeleteUserStrategyCodeCommand(int UserId, int StrategyId, int TradeCodeId) : ICommand;

public class DeleteUserStrategyCodeCommandHandler(IUserStrategyTradeCodeRepository userStrategyTradeCodeRepository)
	: IRequestHandler<DeleteUserStrategyCodeCommand>
{
	public async Task Handle(DeleteUserStrategyCodeCommand request, CancellationToken ct)
	{
		var affectedRows = await userStrategyTradeCodeRepository.ExecuteDeleteAsync(
			e => e.UserId == request.UserId &&
				 e.StrategyId == request.StrategyId &&
				 e.TradeCodeId == request.TradeCodeId,
			ct);

		if (affectedRows == 0)
			throw new KeyNotFoundException("User strategy code not found");
	}
}
