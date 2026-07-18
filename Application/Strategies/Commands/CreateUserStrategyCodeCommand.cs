using Application.Interfaces.Repositories.Database;
using MediatR;

namespace Application.Strategies.Commands;

public record CreateUserStrategyCodeCommand(int UserId, int StrategyId, int TradeCodeId) : IRequest;

public class CreateUserStrategyCodeCommandHandler(IUserStrategyTradeCodeRepository userStrategyTradeCodeRepository) 
	: IRequestHandler<CreateUserStrategyCodeCommand>
{
	private readonly IUserStrategyTradeCodeRepository _userStrategyTradeCodeRepository = userStrategyTradeCodeRepository;

	public async Task Handle(CreateUserStrategyCodeCommand request, CancellationToken cancellationToken)
	{
		bool isUserStrategyCodeExist = await _userStrategyTradeCodeRepository.ExistsAsync(
			e => e.UserId == request.UserId &&
			e.StrategyId == request.StrategyId &&
			e.TradeCodeId == request.TradeCodeId,
			cancellationToken);

		if (isUserStrategyCodeExist)
			throw new InvalidOperationException("User strategy code already exists");

		var newUserStrategyCode = new Domain.Strategies.Entities.UserStrategyTradeCode(
			request.UserId,
			request.TradeCodeId,
			request.StrategyId
		);

		await _userStrategyTradeCodeRepository.AddAsync(newUserStrategyCode, cancellationToken);
		await _userStrategyTradeCodeRepository.SaveChangesAsync(cancellationToken);
	}
}
