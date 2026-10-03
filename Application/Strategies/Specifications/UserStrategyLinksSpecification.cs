using Ardalis.Specification;
using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Common.Specifications;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Strategies.Specifications;

public abstract class UserStrategyLinksSpecification : PageSpecification<UserStrategyInstrument>
{
	protected UserStrategyLinksSpecification(int userId, PageOptions pageOptions)
		: base(pageOptions)
	{
		Query.Where(link => link.UserId == userId);
	}
}
