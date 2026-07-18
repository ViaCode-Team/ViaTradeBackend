using MediatR;

namespace Application.Common.Interfaces;

public interface IQuery<TResponse> : IRequest<TResponse>
{
}
