using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using ViaTrade.Application.Email;
using ViaTrade.Configuration.Options;
using ViaTrade.Infrastructure.Email;

namespace ViaTrade.Tests;

public sealed class SmtpEmailTransportTests
{
	[Fact]
	public async Task LocalSmtpAcceptsMailWithFrontendFragmentLink()
	{
		using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(10));
		var listener = new TcpListener(IPAddress.Loopback, 0);
		listener.Start();
		try
		{
			var port = ((IPEndPoint)listener.LocalEndpoint).Port;
			var receive = ReceiveAsync(listener, deadline.Token);
			var settings = Options.Create(
				new EmailSettings
				{
					SmtpHost = "127.0.0.1",
					SmtpPort = port,
					FromAddress = "security@example.test",
					FrontendUrl = "https://frontend.example.test",
				}
			);
			var message = new EmailMessage(
				Guid.NewGuid(),
				123,
				"recipient@example.test",
				"Registration",
				EmailIdentity.NewSecret(),
				EmailIdentity.UtcNow().AddHours(24)
			);
			await new SmtpEmailTransport(settings).SendAsync(message, deadline.Token);
			var raw = await receive;
			var body = Regex.Replace(raw, "=\\r?\\n", "");
			body = Regex.Replace(
				body,
				"=([A-Fa-f0-9]{2})",
				match =>
					(
						(char)int.Parse(match.Groups[1].Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture)
					).ToString()
			);
			Assert.Contains("https://frontend.example.test/emailConfirmation#token=" + message.Token, body);
			Assert.Contains("X-ViaTrade-Event: " + message.EventId.ToString("N"), body);
			Assert.DoesNotContain("?token=", body);
		}
		finally
		{
			listener.Stop();
		}
	}

	[Fact]
	public async Task ExpiredMessageDoesNotConnectToSmtp()
	{
		var transport = new SmtpEmailTransport(
			Options.Create(new EmailSettings { SmtpHost = "127.0.0.1", SmtpPort = 1 })
		);
		await transport.SendAsync(
			new EmailMessage(
				Guid.NewGuid(),
				1,
				"expired@example.test",
				"Registration",
				EmailIdentity.NewSecret(),
				EmailIdentity.UtcNow().AddSeconds(-1)
			),
			CancellationToken.None
		);
	}

	private static async Task<string> ReceiveAsync(TcpListener listener, CancellationToken ct)
	{
		using var client = await listener.AcceptTcpClientAsync(ct);
		await using var stream = client.GetStream();
		using var reader = new StreamReader(stream, Encoding.ASCII, leaveOpen: true);
		await using var writer = new StreamWriter(stream, Encoding.ASCII, leaveOpen: true)
		{
			AutoFlush = true,
			NewLine = "\r\n",
		};
		await writer.WriteLineAsync("220 localhost SMTP".AsMemory(), ct);
		while (await reader.ReadLineAsync(ct) is string line)
		{
			if (line == "DATA")
			{
				await writer.WriteLineAsync("354 Send message".AsMemory(), ct);
				var data = new StringBuilder();
				while (await reader.ReadLineAsync(ct) is string bodyLine && bodyLine != ".")
					data.AppendLine(bodyLine);
				await writer.WriteLineAsync("250 Accepted by transport".AsMemory(), ct);
				return data.ToString();
			}
			await writer.WriteLineAsync("250 OK".AsMemory(), ct);
		}
		throw new IOException("SMTP connection closed before DATA.");
	}
}
