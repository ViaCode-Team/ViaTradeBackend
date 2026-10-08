using Mediator;

namespace ViaTrade.Application.Auth.Register;

public sealed record RegisterCommand(string Login, string Password) : ICommand;
