using Mediator;
using ViaTrade.Application.Auth.Common;

namespace ViaTrade.Application.Auth.Login;

public sealed record LoginCommand(string Login, string Password, string UserAgent) : ICommand<AuthTokensResult>;
