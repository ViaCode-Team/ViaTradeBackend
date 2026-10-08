using ViaTrade.Application.Common.Models;

namespace ViaTrade.Application.Common.Specifications;

public abstract class PageSpecification<T> : OrderedSpecification<T>
{
	public int Page { get; }
	public int PageSize { get; }
	public int Offset { get; }

	protected PageSpecification(PageOptions pageOptions)
	{
		Page = pageOptions.Page;
		PageSize = pageOptions.PageSize;
		Offset = checked((Page - 1) * PageSize);
	}
}
