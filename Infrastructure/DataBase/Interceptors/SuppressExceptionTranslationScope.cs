namespace ViaTrade.Infrastructure.DataBase.Interceptors;

/// <summary>
/// Controls the execution scope for exception translation in Entity Framework.
///
/// Example usage:
/// <code>
/// using (SuppressExceptionTranslationScope.Create())
/// {
///     // The interceptor will ignore any MySQL exceptions here.
///     // You can catch them manually.
///     await _context.SaveChangesAsync(ct);
/// }
/// </code>
/// </summary>
public sealed class SuppressExceptionTranslationScope : IDisposable
{
	private static readonly AsyncLocal<bool> _isSuppressed = new();

	public static bool IsSuppressed => _isSuppressed.Value;

	private SuppressExceptionTranslationScope()
	{
		_isSuppressed.Value = true;
	}

	public static SuppressExceptionTranslationScope Create()
	{
		return new SuppressExceptionTranslationScope();
	}

	public void Dispose()
	{
		_isSuppressed.Value = false;
	}
}
