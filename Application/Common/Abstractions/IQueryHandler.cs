namespace ViaTrade.Application.Common.Abstractions;

public interface IQueryHandler<in TQuery, TResult>
	where TQuery : IQuery<TResult>
{
	Task<TResult> HandleAsync(TQuery query, CancellationToken ct = default);
}
