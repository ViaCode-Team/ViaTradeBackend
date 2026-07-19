using Application.Common.Interfaces;
using Application.Trades.Interfaces;
using MediatR;

namespace Application.Trades.Commands;

public record DeleteTradeCommand(int Id, int UserId) : ICommand;

public class DeleteTradeCommandHandler(ITradeRepository tradeRepository) : IRequestHandler<DeleteTradeCommand>
{
	public async Task Handle(DeleteTradeCommand request, CancellationToken cancellationToken)
	{
		var affectedRows = await tradeRepository.ExecuteDeleteAsync(t => t.Id == request.Id && t.UserId == request.UserId, cancellationToken);
		if (affectedRows == 0)
		{
			bool exists = await tradeRepository.ExistsAsync(t => t.Id == request.Id, cancellationToken);
			if (exists)
				throw new UnauthorizedAccessException();

			throw new KeyNotFoundException();
		}
	}
}
