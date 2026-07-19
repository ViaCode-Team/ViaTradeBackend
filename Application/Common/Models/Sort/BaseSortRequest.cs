using System.ComponentModel.DataAnnotations;

namespace Application.Common.Models.Sort;

public abstract record BaseSortRequest<TEnum> : IValidatableObject where TEnum : struct, Enum
{
	public List<TEnum> SortBy { get; init; } = [];

	protected virtual List<TEnum> DefaultSortBy => [];

	public List<TEnum> GetEffectiveSortBy() => SortBy.Count > 0 ? SortBy : DefaultSortBy;

	public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
	{
		var effectiveSort = GetEffectiveSortBy();
		if (effectiveSort == null || effectiveSort.Count <= 1)
			yield break;

		var baseFields = effectiveSort
			.Select(x => x
				.ToString()
				.Replace("Asc", "")
				.Replace("Desc", ""))
			.ToList();

		if (baseFields.Distinct().Count() != baseFields.Count)
		{
			yield return new ValidationResult(
				"Duplicate or conflicting sort fields were provided (for example, both ascending and descending order for the same field).",
				[nameof(SortBy)]
			);
		}
	}
}
