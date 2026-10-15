using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using ViaTrade.Application.Email;
using ViaTrade.Configuration.Options;

namespace ViaTrade.Infrastructure.Email;

public sealed class SmtpEmailTransport(IOptions<EmailSettings> options) : IEmailTransport
{
	public async Task SendAsync(EmailMessage message, CancellationToken ct)
	{
		if (message.ExpiresAt <= DateTime.UtcNow)
			return;

		var settings = options.Value;
		var subject = message.Kind switch
		{
			"Registration" => "Confirm your ViaTrade email",
			"PasswordReset" => "Reset your ViaTrade password",
			"EmailChange" => "Confirm your new ViaTrade email",
			"EmailRecovery" => "Your ViaTrade email changed: restore the previous email",
			_ => "ViaTrade account security notification",
		};
		var body =
			"Account security event: "
			+ message.Kind
			+ ". If you did not request this action, review your account security.";
		if (message.Token != null)
		{
			var path = message.Kind switch
			{
				"Registration" => "emailConfirmation",
				"PasswordReset" => "passwordReset",
				"EmailChange" => "emailChangeConfirmation",
				"EmailRecovery" => "emailChangeRecovery",
				_ => throw new InvalidOperationException("Unknown email token purpose."),
			};
			body +=
				$"\n\n{settings.FrontendUrl.TrimEnd('/')}/{path}#token={message.Token}\n\nExpires at {message.ExpiresAt:O}. Opening the page does not apply the action. Follow the page instructions to submit it.";
		}

		using var mail = new MailMessage(settings.FromAddress, message.Recipient, subject, body);
		mail.Headers.Add("X-ViaTrade-Event", message.EventId.ToString("N"));
		using var client = new SmtpClient(settings.SmtpHost, settings.SmtpPort)
		{
			EnableSsl = settings.SmtpEnableSsl,
			Timeout = 30000,
		};
		if (settings.SmtpUsername != null)
			client.Credentials = new NetworkCredential(settings.SmtpUsername, settings.SmtpPassword);
		await client.SendMailAsync(mail, ct);
	}
}
