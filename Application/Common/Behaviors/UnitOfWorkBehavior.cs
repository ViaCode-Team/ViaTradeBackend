using Application.Common.Interfaces;
using MediatR;

namespace Application.Common.Behaviors;

public class UnitOfWorkBehavior<TRequest, TResponse>(IUnitOfWork unitOfWork)
	: IPipelineBehavior<TRequest, TResponse>
	where TRequest : notnull
{
	public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
	{
		if (request is not (ITransactionalCommand or ITransactionalCommand<TResponse>))
			return await next(ct);

		var response = await next(ct);

		await unitOfWork.SaveChangesAsync(ct);

		return response;
	}
}
