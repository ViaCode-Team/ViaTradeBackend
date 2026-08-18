namespace ViaTrade.Api.Routing;

public static class ApiRoutes
{
	public static class V1
	{
		public const string Prefix = "api/v1";
		public const string Web = $"{Prefix}";
		public const string TgBot = $"{Prefix}/internal/{InternalServices.TgBot}";
		public const string Analyzer = $"{Prefix}/internal/{InternalServices.Analyzer}";
	}
}
