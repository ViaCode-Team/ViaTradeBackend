using ViaTrade.Application.Common.Abstractions;

namespace ViaTrade.Application.Users.UpdateLastLogin;

public sealed record UpdateLastLoginCommand(int UserId) : ICommand;
