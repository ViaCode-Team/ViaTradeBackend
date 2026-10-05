using ViaTrade.Application.Auth.Common;
using ViaTrade.Application.Common.Abstractions;

namespace ViaTrade.Application.Auth.Login;

public sealed record LoginCommand(string Login, string Password, string UserAgent) : ICommand<AuthTokensResult>;
