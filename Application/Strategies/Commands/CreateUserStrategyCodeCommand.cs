using Application.Common.Interfaces;
using Application.Strategies.Interfaces;
using Domain.Strategies.Entities;
using MediatR;

namespace Application.Strategies.Commands;

public record CreateUserStrategyCodeCommand(int UserId, int StrategyId, int TradeCodeId) : ITransactionalCommand;

public class CreateUserStrategyCodeCommandHandler(IUserStrategyTradeCodeRepository userStrategyTradeCodeRepository)
	: IRequestHandler<CreateUserStrategyCodeCommand>
{
	public async Task Handle(CreateUserStrategyCodeCommand request, CancellationToken cancellationToken)
	{
		bool isUserStrategyCodeExist = await userStrategyTradeCodeRepository.ExistsAsync(
			e => e.UserId == request.UserId &&
			e.StrategyId == request.StrategyId &&
			e.TradeCodeId == request.TradeCodeId,
			cancellationToken);

		if (isUserStrategyCodeExist)
			throw new InvalidOperationException("User strategy code already exists");

		var newUserStrategyCode = new UserStrategyTradeCode
		{
			UserId = request.UserId,
			TradeCodeId = request.TradeCodeId,
			StrategyId = request.StrategyId
		};

		await userStrategyTradeCodeRepository.AddAsync(newUserStrategyCode, cancellationToken);
	}
}
