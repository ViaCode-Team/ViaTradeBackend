using Microsoft.Extensions.Options;
using ViaTrade.Application.Common.Exceptions;
using ViaTrade.Configuration.Options;

namespace ViaTrade.Application.Email;

public sealed class EmailRequestLimiter(IEmailQueue queue, IOptions<EmailSettings> options)
{
	public Task RequireIpAsync(string ip, CancellationToken ct) =>
		RequireAsync("ip", ip, options.Value.IpRequestsPerMinute, TimeSpan.FromMinutes(1), ct);

	public Task RequireLoginAsync(string identity, CancellationToken ct) =>
		RequireAsync(
			"login",
			EmailIdentity.NormalizeLogin(identity ?? ""),
			options.Value.LoginAttemptsPerFiveMinutes,
			TimeSpan.FromMinutes(5),
			ct
		);

	public Task RequireTokenAsync(string ip, CancellationToken ct) =>
		RequireAsync("token", ip, options.Value.TokenAttemptsPerMinute, TimeSpan.FromMinutes(1), ct);

	public Task RequireAccountAsync(int userId, CancellationToken ct) =>
		RequireAsync("account", userId.ToString(), options.Value.AccountRequestsPerHour, TimeSpan.FromHours(1), ct);

	private async Task RequireAsync(string scope, string value, int limit, TimeSpan window, CancellationToken ct)
	{
		if (!await queue.TryAcquireLimitAsync(scope, value, limit, window, ct))
			throw new RateLimitException();
	}
}
