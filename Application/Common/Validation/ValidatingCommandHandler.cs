using FluentValidation;
using ViaTrade.Application.Common.Abstractions;

namespace ViaTrade.Application.Common.Validation;

public sealed class ValidatingCommandHandler<TCommand>(
	ICommandHandler<TCommand> inner,
	IEnumerable<IValidator<TCommand>> validators
) : ICommandHandler<TCommand>
	where TCommand : ICommand
{
	private readonly ICommandHandler<TCommand> _inner = inner;

	public async Task HandleAsync(TCommand request, CancellationToken ct)
	{
		await RequestValidation.ValidateAsync(request, validators, ct);

		await _inner.HandleAsync(request, ct);
	}
}
