using Microsoft.Extensions.Options;

namespace ViaTrade.Configuration.Options;

public sealed class ApplicationSettings
{
	[ValidateObjectMembers]
	public ConnectionStringsSettings ConnectionStrings { get; set; } = new();

	[ValidateObjectMembers]
	public JwtSettings Jwt { get; set; } = new();

	[ValidateObjectMembers]
	public AuthCookieSettings AuthCookies { get; set; } = new();

	[ValidateObjectMembers]
	public TelegramBotSettings TelegramBot { get; set; } = new();

	[ValidateObjectMembers]
	public NotificationStreamSettings TelegramNotifications { get; set; } = new();

	[ValidateObjectMembers]
	public ReminderCleanupSettings ReminderCleanup { get; set; } = new();

	[ValidateObjectMembers]
	public ReminderLimitsSettings ReminderLimits { get; set; } = new();

	[ValidateObjectMembers]
	public AnalyzerDataSettings AnalyzerData { get; set; } = new();

	[ValidateObjectMembers]
	public ServiceSecuritySettings ServiceSecurity { get; set; } = new();
}
