using Application.Common.Interfaces;
using MediatR;

namespace Application.Common.Behaviors;

public class UnitOfWorkBehavior<TRequest, TResponse>(IUnitOfWork unitOfWork)
	: IPipelineBehavior<TRequest, TResponse>
	where TRequest : notnull
{
	public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
	{
		if (request is not (ITransactionalCommand or ITransactionalCommand<TResponse>))
			return await next(cancellationToken);

		var response = await next(cancellationToken);

		await unitOfWork.SaveChangesAsync(cancellationToken);

		return response;
	}
}
