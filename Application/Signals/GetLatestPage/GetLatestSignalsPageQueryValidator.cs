using ViaTrade.Application.Common.Validation;
using ViaTrade.Application.Signals.Common;

namespace ViaTrade.Application.Signals.GetLatestPage;

public sealed class GetLatestSignalsPageQueryValidator : UserRequestValidator<GetLatestSignalsPageQuery>
{
	public GetLatestSignalsPageQueryValidator()
		: base(request => request.UserId)
	{
		RuleFor(request => request.PageOptions).ValidPageOptions();
		RuleFor(request => request.SignalSort).RequiredValid(new SortValidator<SignalSort, SignalSortField>());
	}
}
