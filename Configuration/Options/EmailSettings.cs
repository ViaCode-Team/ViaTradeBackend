using System.ComponentModel.DataAnnotations;

namespace ViaTrade.Configuration.Options;

public sealed class EmailSettings
{
	[Range(0, 15)]
	public int RedisDatabase { get; set; } = 2;

	[Required]
	public string StreamPrefix { get; set; } = "email";

	[Required, Url]
	public string FrontendUrl { get; set; } = "";

	public string[] AllowedOrigins { get; set; } = [];

	[Required]
	public string DataProtectionKeyPath { get; set; } = "";

	[Required]
	public string SmtpHost { get; set; } = "";

	[Range(1, 65535)]
	public int SmtpPort { get; set; } = 1025;

	[Required, EmailAddress]
	public string FromAddress { get; set; } = "";

	public bool SmtpEnableSsl { get; set; }

	public string? SmtpUsername { get; set; }

	public string? SmtpPassword { get; set; }

	[Range(1, 100)]
	public int BatchSize { get; set; } = 20;

	[Range(100, 1000000)]
	public int MaxQueuedMessages { get; set; } = 100000;

	[Range(1, 20)]
	public int MaxAttempts { get; set; } = 8;

	[Range(1, 60)]
	public int JobWaitMinutes { get; set; } = 30;

	[Range(1, 60)]
	public int RetryBaseSeconds { get; set; } = 5;

	[Range(1, 3600)]
	public int RetryMaxSeconds { get; set; } = 300;

	[Range(1, 3600)]
	public int IpRequestsPerMinute { get; set; } = 30;

	[Range(1, 100)]
	public int LoginAttemptsPerFiveMinutes { get; set; } = 10;

	[Range(1, 100)]
	public int TokenAttemptsPerMinute { get; set; } = 10;

	[Range(1, 1000)]
	public int AccountRequestsPerHour { get; set; } = 30;
}
