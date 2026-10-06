using ViaTrade.Application.Common.Validation;
using ViaTrade.Application.Signals.Common;

namespace ViaTrade.Application.Signals.GetHistoryPage;

public sealed class GetSignalHistoryPageQueryValidator : UserRequestValidator<GetSignalHistoryPageQuery>
{
	public GetSignalHistoryPageQueryValidator()
		: base(request => request.UserId)
	{
		RuleFor(request => request.SignalHistoryFilter).RequiredValid(new SignalHistoryFilterValidator());
		RuleFor(request => request.PageOptions).ValidPageOptions();
		RuleFor(request => request.SignalSort).RequiredValid(new SortValidator<SignalSort, SignalSortField>());
	}
}
