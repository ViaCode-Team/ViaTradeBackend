namespace Application.Auth.Models;

public sealed class SessionLifetimeOptions
{
	public required TimeSpan IdleLifetime { get; init; }

	public required TimeSpan AbsoluteLifetime { get; init; }
}
