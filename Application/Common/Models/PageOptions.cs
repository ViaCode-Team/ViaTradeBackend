using System.ComponentModel;

namespace ViaTrade.Application.Common.Models;

public class PageOptions
{
	public const int MaxPageSize = 100;
	public const int MaxPage = int.MaxValue / MaxPageSize + 1;

	[DefaultValue(1)]
	public int Page { get; set; } = 1;

	[DefaultValue(20)]
	public int PageSize { get; set; } = 20;
}
