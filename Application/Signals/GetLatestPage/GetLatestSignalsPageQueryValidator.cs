using FluentValidation;
using ViaTrade.Application.Common.Validation;
using ViaTrade.Application.Signals.Common;

namespace ViaTrade.Application.Signals.GetLatestPage;

public sealed class GetLatestSignalsPageQueryValidator : AbstractValidator<GetLatestSignalsPageQuery>
{
	public GetLatestSignalsPageQueryValidator()
	{
		RuleFor(request => request.PageOptions).ValidPageOptions();
		RuleFor(request => request.SignalSort).RequiredValid(new SortValidator<SignalSort, SignalSortField>());
	}
}
