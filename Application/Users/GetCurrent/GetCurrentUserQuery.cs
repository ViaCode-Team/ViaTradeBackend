using Mediator;

namespace ViaTrade.Application.Users.GetCurrent;

public sealed record GetCurrentUserQuery() : IQuery<CurrentUserResult>;
