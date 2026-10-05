using ViaTrade.Application.Auth.Common;
using ViaTrade.Application.Common.Abstractions;

namespace ViaTrade.Application.Auth.RefreshTokens;

public sealed record RefreshTokensCommand(string RefreshToken) : ICommand<AuthTokensResult>;
