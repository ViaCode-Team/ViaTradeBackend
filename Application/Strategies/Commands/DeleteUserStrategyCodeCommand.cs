using Application.Common.Interfaces;
using Application.Strategies.Interfaces;
using MediatR;

namespace Application.Strategies.Commands;

public record DeleteUserStrategyCodeCommand(int UserId, int StrategyId, int TradeCodeId) : ICommandWithoutUoW;

public class DeleteUserStrategyCodeCommandHandler(IUserStrategyTradeCodeRepository userStrategyTradeCodeRepository)
	: IRequestHandler<DeleteUserStrategyCodeCommand>
{
	private readonly IUserStrategyTradeCodeRepository _userStrategyTradeCodeRepository = userStrategyTradeCodeRepository;

	public async Task Handle(DeleteUserStrategyCodeCommand request, CancellationToken cancellationToken)
	{
		var affectedRows = await _userStrategyTradeCodeRepository.ExecuteDeleteAsync(
			e => e.UserId == request.UserId &&
				 e.StrategyId == request.StrategyId &&
				 e.TradeCodeId == request.TradeCodeId,
			cancellationToken);

		if (affectedRows == 0)
			throw new KeyNotFoundException("User strategy code not found");
	}
}
