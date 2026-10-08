using FluentValidation;

namespace ViaTrade.Application.Common.Models;

public sealed class PageOptionsValidator : AbstractValidator<PageOptions>
{
	public PageOptionsValidator()
	{
		RuleFor(options => options.Page).InclusiveBetween(1, PageOptions.MaxPage);
		RuleFor(options => options.PageSize).InclusiveBetween(1, PageOptions.MaxPageSize);
	}
}
