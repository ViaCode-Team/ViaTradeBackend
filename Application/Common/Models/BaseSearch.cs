using System.ComponentModel.DataAnnotations;

namespace ViaTrade.Application.Common.Models;

public abstract class BaseSearch
{
	[StringLength(100)]
	public virtual string? SearchText { get; init; }

	public virtual string? GetNormalizedSearchText()
	{
		if (string.IsNullOrWhiteSpace(SearchText))
			return null;

		return SearchText.Trim();
	}
}
