using ViaTrade.Application.Common.Abstractions;

namespace ViaTrade.Application.Auth.LogoutAll;

public sealed record LogoutAllCommand(int UserId) : ICommand;
