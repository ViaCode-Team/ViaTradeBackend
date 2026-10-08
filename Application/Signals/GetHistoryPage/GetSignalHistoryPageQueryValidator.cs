using FluentValidation;
using ViaTrade.Application.Common.Validation;
using ViaTrade.Application.Signals.Common;

namespace ViaTrade.Application.Signals.GetHistoryPage;

public sealed class GetSignalHistoryPageQueryValidator : AbstractValidator<GetSignalHistoryPageQuery>
{
	public GetSignalHistoryPageQueryValidator()
	{
		RuleFor(request => request.SignalHistoryFilter).RequiredValid(new SignalHistoryFilterValidator());
		RuleFor(request => request.PageOptions).ValidPageOptions();
		RuleFor(request => request.SignalSort).RequiredValid(new SortValidator<SignalSort, SignalSortField>());
	}
}
