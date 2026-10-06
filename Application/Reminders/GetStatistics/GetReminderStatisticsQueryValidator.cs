using ViaTrade.Application.Common.Validation;

namespace ViaTrade.Application.Reminders.GetStatistics;

public sealed class GetReminderStatisticsQueryValidator : UserRequestValidator<GetReminderStatisticsQuery>
{
	public GetReminderStatisticsQueryValidator()
		: base(request => request.UserId) { }
}
