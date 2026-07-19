namespace Application.Common.Interfaces;

public interface ITransactionalCommand : ICommand
{
}

public interface ITransactionalCommand<TResponse> : ICommand<TResponse>
{
}
