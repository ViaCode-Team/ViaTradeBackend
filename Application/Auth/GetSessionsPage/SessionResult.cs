using ViaTrade.Application.Auth.Common;

namespace ViaTrade.Application.Auth.GetSessionsPage;

public sealed record SessionResult(string Id, int UserId, string UserAgent, DateTime CreatedAt, DateTime LastSeen)
{
	public static Func<SessionData, SessionResult> Projection { get; } =
		session => new SessionResult(session.Id, session.UserId, session.UserAgent, session.CreatedAt, session.LastSeen);
}
