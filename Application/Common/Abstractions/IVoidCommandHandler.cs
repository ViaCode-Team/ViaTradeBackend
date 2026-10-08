using Mediator;

namespace ViaTrade.Application.Common.Abstractions;

public interface IVoidCommandHandler<in TCommand> : ICommandHandler<TCommand>
	where TCommand : ICommand
{
	new ValueTask Handle(TCommand command, CancellationToken ct);

	async ValueTask<Unit> ICommandHandler<TCommand, Unit>.Handle(TCommand command, CancellationToken ct)
	{
		await Handle(command, ct);

		return Unit.Value;
	}
}
