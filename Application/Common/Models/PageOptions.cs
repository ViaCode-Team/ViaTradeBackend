using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ViaTrade.Application.Common.Models;

public class PageOptions
{
	public const int MaxPageSize = 100;
	public const int MaxPage = int.MaxValue / MaxPageSize + 1;

	[DefaultValue(1), Range(1, MaxPage)]
	public int Page { get; set; } = 1;

	[DefaultValue(20), Range(1, MaxPageSize)]
	public int PageSize { get; set; } = 20;
}
