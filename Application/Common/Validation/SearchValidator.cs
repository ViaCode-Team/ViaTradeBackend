using FluentValidation;
using ViaTrade.Application.Common.Models;

namespace ViaTrade.Application.Common.Validation;

internal sealed class SearchValidator<TSearch> : AbstractValidator<TSearch>
	where TSearch : BaseSearch
{
	public SearchValidator()
	{
		RuleFor(search => search.SearchText).MaximumLength(100);
	}
}
