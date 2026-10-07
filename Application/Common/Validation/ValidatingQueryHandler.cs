using FluentValidation;
using ViaTrade.Application.Common.Abstractions;

namespace ViaTrade.Application.Common.Validation;

public sealed class ValidatingQueryHandler<TQuery, TResult>(
	IQueryHandler<TQuery, TResult> inner,
	IEnumerable<IValidator<TQuery>> validators
) : IQueryHandler<TQuery, TResult>
	where TQuery : IQuery<TResult>
{
	private readonly IQueryHandler<TQuery, TResult> _inner = inner;

	public async Task<TResult> HandleAsync(TQuery request, CancellationToken ct)
	{
		await RequestValidation.ValidateAsync(request, validators, ct);

		return await _inner.HandleAsync(request, ct);
	}
}
