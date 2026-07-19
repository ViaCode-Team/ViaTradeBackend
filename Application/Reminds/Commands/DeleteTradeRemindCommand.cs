using Application.Common.Interfaces;
using Application.Reminds.Interfaces;
using MediatR;

namespace Application.Reminds.Commands;

public record DeleteTradeRemindCommand(int RemindId, int UserId) : ICommand;

public class DeleteTradeRemindCommandHandler(ITradeRemindRepository repository) : IRequestHandler<DeleteTradeRemindCommand>
{
	public async Task Handle(DeleteTradeRemindCommand request, CancellationToken cancellationToken)
	{
		var rows = await repository.ExecuteDeleteAsync(
			x => x.Id == request.RemindId && x.UserId == request.UserId,
			cancellationToken);

		if (rows == 0)
			throw new Exception("Remind not found.");
	}
}
