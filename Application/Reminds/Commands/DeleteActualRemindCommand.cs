using Application.Interfaces.Repositories.Database;
using MediatR;

namespace Application.Reminds.Commands;

public record DeleteActualRemindCommand(int RemindId) : IRequest;

public class DeleteActualRemindCommandHandler(ITradeRemindRepository repository) : IRequestHandler<DeleteActualRemindCommand>
{
    public async Task Handle(DeleteActualRemindCommand request, CancellationToken cancellationToken)
    {
        var remind = await repository.GetByIdAsync(request.RemindId, cancellationToken);

        if (remind == null)
        {
            throw new Exception("Remind not found.");
        }

        repository.Remove(remind);
        await repository.SaveChangesAsync(cancellationToken);
    }
}
