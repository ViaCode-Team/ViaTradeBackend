using Application.Interfaces.Repositories.Database;
using MediatR;

namespace Application.Reminds.Commands;

public record DeleteTradeRemindCommand(int RemindId, int UserId) : IRequest;

public class DeleteTradeRemindCommandHandler(ITradeRemindRepository repository) : IRequestHandler<DeleteTradeRemindCommand>
{
    public async Task Handle(DeleteTradeRemindCommand request, CancellationToken cancellationToken)
    {
        var reminds = await repository.FindAsync(x => x.Id == request.RemindId && x.UserId == request.UserId, cancellationToken);
        var remind = reminds.FirstOrDefault();

        if (remind == null)
        {
            throw new Exception("Remind not found.");
        }

        repository.Remove(remind);
        await repository.SaveChangesAsync(cancellationToken);
    }
}
