using Application.Common.Interfaces;
using Application.Reminds.Interfaces;
using MediatR;

namespace Application.Reminds.Commands;

public record DeleteActualRemindCommand(int RemindId) : ICommand;

public class DeleteActualRemindCommandHandler(ITradeRemindRepository repository) : IRequestHandler<DeleteActualRemindCommand>
{
	public async Task Handle(DeleteActualRemindCommand request, CancellationToken ct)
	{
		var rows = await repository.ExecuteDeleteAsync(
			x => x.Id == request.RemindId,
			ct);

		if (rows == 0)
			throw new Exception("Remind not found.");
	}
}
