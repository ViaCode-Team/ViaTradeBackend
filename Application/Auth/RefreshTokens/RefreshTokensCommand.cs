using Mediator;
using ViaTrade.Application.Auth.Common;

namespace ViaTrade.Application.Auth.RefreshTokens;

public sealed record RefreshTokensCommand(string RefreshToken) : ICommand<AuthTokensResult>;
