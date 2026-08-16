namespace ViaTrade.Infrastructure.Redis.Scripts;

internal static class SessionRedisScripts
{
	private const string ResourcePrefix = "Infrastructure.Redis.Scripts.";

	public static readonly string RotateRefresh = ReadEmbeddedScript("rotate_refresh.lua");
	public static readonly string TerminateSession = ReadEmbeddedScript("terminate_session.lua");

	private static string ReadEmbeddedScript(string fileName)
	{
		var resourceName = $"{ResourcePrefix}{fileName}";
		using var stream = typeof(SessionRedisScripts).Assembly.GetManifestResourceStream(resourceName);
		if (stream == null)
			throw new InvalidOperationException($"Redis script resource '{resourceName}' was not found.");

		using var reader = new StreamReader(stream);
		return reader.ReadToEnd();
	}
}
