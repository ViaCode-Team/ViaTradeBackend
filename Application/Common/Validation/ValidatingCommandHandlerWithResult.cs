using FluentValidation;
using ViaTrade.Application.Common.Abstractions;

namespace ViaTrade.Application.Common.Validation;

public sealed class ValidatingCommandHandler<TCommand, TResult>(
	ICommandHandler<TCommand, TResult> inner,
	IEnumerable<IValidator<TCommand>> validators
) : ICommandHandler<TCommand, TResult>
	where TCommand : ICommand<TResult>
{
	private readonly ICommandHandler<TCommand, TResult> _inner = inner;

	public async Task<TResult> HandleAsync(TCommand request, CancellationToken ct = default)
	{
		await RequestValidation.ValidateAsync(request, validators, ct);

		return await _inner.HandleAsync(request, ct);
	}
}
