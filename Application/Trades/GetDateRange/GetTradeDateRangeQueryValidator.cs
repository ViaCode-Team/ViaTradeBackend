using ViaTrade.Application.Common.Validation;

namespace ViaTrade.Application.Trades.GetDateRange;

public sealed class GetTradeDateRangeQueryValidator : UserRequestValidator<GetTradeDateRangeQuery>
{
	public GetTradeDateRangeQueryValidator()
		: base(request => request.UserId) { }
}
