using System.ComponentModel.DataAnnotations;

namespace ViaTrade.Application.Common.Models;

public abstract record Sort<TField> : IValidatableObject
	where TField : struct, Enum
{
	public List<TField> SortBy { get; init; } = [];

	protected virtual List<TField> DefaultSortBy => [];

	public List<TField> GetEffectiveSortBy()
	{
		if (SortBy.Count > 0)
		{
			return SortBy;
		}

		return DefaultSortBy;
	}

	public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
	{
		var effectiveSort = GetEffectiveSortBy();
		if (effectiveSort.Count <= 1)
			yield break;

		var baseFields = effectiveSort.Select(x => x.ToString().Replace("Asc", "").Replace("Desc", "")).ToList();

		if (baseFields.Distinct().Count() != baseFields.Count)
		{
			yield return new ValidationResult(
				"Duplicate or conflicting sort fields were provided (for example, both ascending and descending order for the same field).",
				[nameof(SortBy)]
			);
		}
	}
}
