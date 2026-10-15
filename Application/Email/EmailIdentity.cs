using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using ViaTrade.Application.Common.Exceptions;

namespace ViaTrade.Application.Email;

public static class EmailIdentity
{
	public static DateTime UtcNow() => new(DateTime.UtcNow.Ticks / 10 * 10, DateTimeKind.Utc);

	public static string NormalizeLogin(string login) => login.Trim().ToUpperInvariant();

	public static string NormalizeEmail(string email) => email.Trim().ToUpperInvariant();

	public static bool IsEmail(string email)
	{
		var parsed = MailAddress.TryCreate(email, out var address);
		return parsed && address != null && address.Address == email && email.Length <= 254;
	}

	public static void ValidatePassword(string password)
	{
		if (string.IsNullOrWhiteSpace(password) || password.Length < 8 || Encoding.UTF8.GetByteCount(password) > 72)
			throw new BadRequestException(
				"Password must contain at least 8 characters and at most 72 UTF-8 bytes.",
				"invalid_password"
			);
	}

	public static string NewSecret() => Convert.ToHexString(RandomNumberGenerator.GetBytes(32));

	public static string Fingerprint(string secret) =>
		Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(secret)));

	public static Guid EventId(Guid operationId, string purpose) =>
		new(SHA256.HashData(Encoding.UTF8.GetBytes($"{operationId:N}:{purpose}")).AsSpan(0, 16));
}
