using System.Linq.Expressions;
using Ardalis.Specification;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Notes.Specifications;

public sealed class NoteWithTargetsSpecification : Specification<Note>
{
	public NoteWithTargetsSpecification(int userId, Expression<Func<Note, bool>> predicate)
	{
		Query
			.Where(note => note.UserId == userId)
			.Where(predicate)
			.Include(note => note.Instrument)
			.Include(note => note.Strategy);
	}
}
