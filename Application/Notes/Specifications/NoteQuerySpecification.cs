using Application.Notes.Models;
using Domain.Entities;
using Domain.Enums;

namespace Application.Common.Specifications;

public class NoteQuerySpecification : BaseQuerySpecification<Note>
{
	public NoteQuerySpecification(int userId, NoteFilter request)
	{
		AddCriteria(x => x.UserId == userId);

		if (request.Target.HasValue)
		{
			var target = request.Target.Value;
			if (target == NoteType.InstrumentNote)
				AddCriteria(x => x.InstrumentId != null);
			else if (target == NoteType.StrategyNote)
				AddCriteria(x => x.StrategyId != null);
		}
	}
}
