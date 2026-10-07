using ViaTrade.Application.Common.Abstractions;

namespace ViaTrade.Application.Users.GetCurrent;

public sealed record GetCurrentUserQuery() : IQuery<CurrentUserResult>;
