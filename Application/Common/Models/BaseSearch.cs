namespace ViaTrade.Application.Common.Models;

public abstract class BaseSearch
{
	public virtual string? SearchText { get; init; }

	public virtual string? GetNormalizedSearchText()
	{
		if (string.IsNullOrWhiteSpace(SearchText))
			return null;

		return SearchText.Trim();
	}
}
