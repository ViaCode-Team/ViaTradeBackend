using FluentValidation;
using ViaTrade.Application.Common.Models;

namespace ViaTrade.Application.Common.Validation;

internal sealed class SortValidator<TSort, TField> : AbstractValidator<TSort>
	where TSort : Sort<TField>
	where TField : struct, Enum
{
	public SortValidator()
	{
		RuleFor(sort => sort.SortBy).NotNull();
		RuleForEach(sort => sort.SortBy).IsInEnum();
		RuleFor(sort => sort.SortBy)
			.Must(fields =>
			{
				if (fields is null)
					return true;

				var names = fields.Select(field => field.ToString().Replace("Asc", "").Replace("Desc", ""));
				return names.Distinct(StringComparer.Ordinal).Count() == fields.Count;
			})
			.WithMessage(
				"Duplicate or conflicting sort fields were provided (for example, both ascending and descending order for the same field)."
			);
	}
}
