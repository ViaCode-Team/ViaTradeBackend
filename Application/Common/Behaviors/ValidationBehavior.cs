using FluentValidation;
using MediatR;
using ValidationException = Application.Common.Exceptions.ValidationException;

namespace Application.Common.Behaviors;

public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
	: IPipelineBehavior<TRequest, TResponse>
	where TRequest : notnull
{
	public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
	{
		if (!validators.Any())
			return await next(ct);

		var context = new ValidationContext<TRequest>(request);

		var validationResults = await Task.WhenAll(
			validators.Select(v => v.ValidateAsync(context, ct)));

		var failures = validationResults
			.Where(r => r.Errors.Count != 0)
			.SelectMany(r => r.Errors)
			.ToList();

		if (failures.Count != 0)
			throw new ValidationException(failures);

		return await next(ct);
	}
}
