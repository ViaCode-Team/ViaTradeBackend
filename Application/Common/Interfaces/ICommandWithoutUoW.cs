namespace Application.Common.Interfaces;

public interface ICommandWithoutUoW : ICommand
{
}

public interface ICommandWithoutUoW<TResponse> : ICommand<TResponse>
{
}
