using ViaTrade.Application.Common.Validation;

namespace ViaTrade.Application.Notes.GetStatistics;

public sealed class GetNoteStatisticsQueryValidator : UserRequestValidator<GetNoteStatisticsQuery>
{
	public GetNoteStatisticsQueryValidator()
		: base(request => request.UserId) { }
}
