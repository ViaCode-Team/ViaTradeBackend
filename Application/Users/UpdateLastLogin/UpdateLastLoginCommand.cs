using Mediator;

namespace ViaTrade.Application.Users.UpdateLastLogin;

public sealed record UpdateLastLoginCommand(int UserId) : ICommand;
