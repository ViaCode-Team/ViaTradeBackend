using FluentValidation;
using Mediator;

namespace ViaTrade.Application.Common.Validation;

public sealed class ValidationBehavior<TMessage, TResult>(IEnumerable<IValidator<TMessage>> validators)
	: IPipelineBehavior<TMessage, TResult>
	where TMessage : notnull, IMessage
{
	public async ValueTask<TResult> Handle(
		TMessage message,
		MessageHandlerDelegate<TMessage, TResult> next,
		CancellationToken ct
	)
	{
		await RequestValidation.ValidateAsync(message, validators, ct);

		return await next(message, ct);
	}
}
