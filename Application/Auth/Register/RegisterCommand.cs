using ViaTrade.Application.Auth.Common;
using ViaTrade.Application.Common.Abstractions;

namespace ViaTrade.Application.Auth.Register;

public sealed record RegisterCommand(string Login, string Password, string UserAgent) : ICommand<AuthTokensResult>;
