using System.Text.Json;
using FluentValidation;
using FluentValidation.Results;
using AppValidationException = ViaTrade.Application.Common.Exceptions.ValidationException;

namespace ViaTrade.Application.Common.Validation;

internal static class RequestValidation
{
	public static async Task ValidateAsync<T>(T request, IEnumerable<IValidator<T>> validators, CancellationToken ct)
	{
		ct.ThrowIfCancellationRequested();
		var failures = new List<ValidationFailure>();

		foreach (var validator in validators)
		{
			var result = await validator.ValidateAsync(request, ct);
			failures.AddRange(result.Errors);
		}

		if (failures.Count == 0)
			return;

		var errors = failures
			.GroupBy(failure =>
				string.Join('.', failure.PropertyName.Split('.').Select(JsonNamingPolicy.CamelCase.ConvertName))
			)
			.ToDictionary(
				group => group.Key,
				group => group.Select(failure => failure.ErrorMessage).Distinct().ToArray()
			);

		throw new AppValidationException("One or more validation errors occurred.", errors);
	}
}
