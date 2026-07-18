using Domain.Notes.Enums;
using Domain.Notes.Entities;
using Domain.Models.Filters;

namespace Application.Specifications;

public class NoteQuerySpecification : BaseQuerySpecification<Note>
{
	public NoteQuerySpecification(int userId, NoteFilterRequest? request)
	{
		AddCriteria(x => x.UserId == userId);

		if (request == null) 
			return;

		if (request.Target.HasValue)
		{
			var target = request.Target.Value;
			if (target == NoteType.TradeCodeNote)
				AddCriteria(x => x.TradeCodeId != null);
			else if (target == NoteType.TradeStrategyNote)
				AddCriteria(x => x.TradeStrategyId != null);
		}
	}
}
