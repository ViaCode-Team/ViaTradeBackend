using Domain.Entities.DataBase;
using Domain.Models.Filters;

namespace Application.Specifications;

public class NoteSpecification : BaseSpecification<Note>
{
	public NoteSpecification(int userId, NoteFilterRequest? request)
	{
		ApplyNoTracking();
		AddCriteria(x => x.UserId == userId);

		if (request == null) return;

		if (request.Target is NoteType target)
		{
			if (target == NoteType.TradeCodeNote)
				AddCriteria(x => x.TradeCodeId != null);
			else if (target == NoteType.TradeStrategyNote)
				AddCriteria(x => x.TradeStrategyId != null);
		}
	}
}
