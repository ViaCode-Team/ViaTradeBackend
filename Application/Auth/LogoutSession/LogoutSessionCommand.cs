using ViaTrade.Application.Common.Abstractions;

namespace ViaTrade.Application.Auth.LogoutSession;

public sealed record LogoutSessionCommand(string SessionId) : ICommand;
