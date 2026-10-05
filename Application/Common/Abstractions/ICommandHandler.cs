namespace ViaTrade.Application.Common.Abstractions;

public interface ICommandHandler<in TCommand>
	where TCommand : ICommand
{
	Task HandleAsync(TCommand command, CancellationToken ct = default);
}

public interface ICommandHandler<in TCommand, TResult>
	where TCommand : ICommand<TResult>
{
	Task<TResult> HandleAsync(TCommand command, CancellationToken ct = default);
}
