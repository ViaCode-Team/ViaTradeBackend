using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Common.Specifications;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Strategies.Specifications;

public sealed class StrategySearchSpecification(SearchFilter filter)
	: SearchSpecification<Strategy, SearchFilter>(filter)
{
	public override IQueryable<Strategy> Apply(IQueryable<Strategy> query)
	{
		if (!string.IsNullOrWhiteSpace(Filter.SearchText))
		{
			var searchText = Filter.SearchText;

			query = query.Where(x =>
				(x.Name != null && x.Name.Contains(searchText)) ||
				x.Description != null && x.Description.Contains(searchText) ||
				x.SignalFrequency != null && x.SignalFrequency.Contains(searchText) ||
				x.InvestmentHorizon != null && x.InvestmentHorizon.Contains(searchText) ||
				x.LogicDescription != null && x.LogicDescription.Contains(searchText) ||
				x.UsageDescription != null && x.UsageDescription.Contains(searchText) ||
				x.LimitationsDescription != null && x.LimitationsDescription.Contains(searchText));
		}

		return query;
	}
}
