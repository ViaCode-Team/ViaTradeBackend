namespace ViaTrade.Application.Common.Abstractions;

public interface IUnitOfWork
{
	Task<int> SaveChangesAsync(CancellationToken ct = default);
}
