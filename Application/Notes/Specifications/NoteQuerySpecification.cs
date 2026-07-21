using Application.Notes.Models;
using Domain.Notes.Entities;
using Domain.Notes.Enums;

namespace Application.Common.Specifications;

public class NoteQuerySpecification : BaseQuerySpecification<Note>
{
	public NoteQuerySpecification(int userId, NoteFilter request)
	{
		AddCriteria(x => x.UserId == userId);

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
