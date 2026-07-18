using Application.Common.Interfaces;
using Application.Reminds.Interfaces;
using MediatR;

namespace Application.Reminds.Commands;

public record DeleteActualRemindCommand(int RemindId) : ICommandWithoutUoW;

public class DeleteActualRemindCommandHandler(ITradeRemindRepository repository) : IRequestHandler<DeleteActualRemindCommand>
{
	public async Task Handle(DeleteActualRemindCommand request, CancellationToken cancellationToken)
	{
		var rows = await repository.ExecuteDeleteAsync(
			x => x.Id == request.RemindId,
			cancellationToken);

		if (rows == 0)
			throw new Exception("Remind not found.");
	}
}
