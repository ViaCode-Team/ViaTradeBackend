using System.ComponentModel.DataAnnotations;

namespace Application.Common.Models.Sort;

public abstract record BaseSortRequest<TEnum> : IValidatableObject where TEnum : struct, Enum
{
	public List<TEnum> SortBy { get; init; } = [];

	public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
	{
		if (SortBy == null || SortBy.Count <= 1) yield break;

		var baseFields = SortBy.Select(x => x.ToString().Replace("Asc", "").Replace("Desc", "")).ToList();

		if (baseFields.Distinct().Count() != baseFields.Count)
		{
			yield return new ValidationResult(
				"В сортировке переданы дублирующиеся или взаимоисключающие поля (например, asc и desc для одного поля).",
				[nameof(SortBy)]
			);
		}
	}
}
