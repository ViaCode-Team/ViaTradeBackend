namespace ViaTrade.Application.Common.Models;

public abstract record Sort<TField>
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
}
