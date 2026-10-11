using System.Text.Json;
using Mediator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using ViaTrade.Api.BackgroundServices;
using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Notifications.Common;
using ViaTrade.Application.Reminders.ListDue;
using ViaTrade.Application.Reminders.MarkPublished;
using ViaTrade.Configuration.Options;
using ViaTrade.Infrastructure.Notifications;
using INotificationPublisher = ViaTrade.Application.Notifications.Common.Abstractions.INotificationPublisher;

namespace ViaTrade.Tests;

public sealed class TelegramReminderPublisherTests
{
	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public async Task PublishesInstrumentIdAndPreservesStreamContract(bool hasInstrument)
	{
		var entries = new List<NameValueEntry[]>();
		var streamSettings = Options.Create(
			new NotificationStreamSettings
			{
				StreamName = "telegram:notifications",
				MaxLength = 1000,
				ReminderPublishIntervalSeconds = 60,
				ReminderPublishBatchSize = 10,
			}
		);
		var database = InterfaceDouble.Create<IDatabase>(
			(method, args) =>
			{
				Assert.Equal("StreamAddAsync", method.Name);
				Assert.Equal(streamSettings.Value.StreamName, ((RedisKey)args[0]!).ToString());
				entries.Add((NameValueEntry[])args[1]!);
				return Task.FromResult((RedisValue)"1-0");
			}
		);
		var connection = InterfaceDouble.Create<IConnectionMultiplexer>(
			(method, args) =>
			{
				Assert.Equal("GetDatabase", method.Name);
				return database;
			}
		);
		var publisher = new RedisStreamNotificationPublisher(connection, streamSettings);
		var events = new List<string>();
		var marked = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
		InstrumentBriefResult? instrument = null;
		if (hasInstrument)
			instrument = new InstrumentBriefResult(123, "AAPL", "Apple");
		var reminder = new DueReminderResult(
			42,
			"Buy < 100 & hold",
			new DateTime(2026, 10, 11, 12, 0, 0, DateTimeKind.Utc),
			instrument,
			7,
			"987654321"
		);
		var sender = InterfaceDouble.Create<ISender>(
			(method, args) =>
			{
				if (args[0] is ListDueRemindersQuery query)
				{
					Assert.Equal(10, query.Limit);
					return new ValueTask<IReadOnlyList<DueReminderResult>>([reminder]);
				}
				var command = Assert.IsType<MarkReminderPublishedCommand>(args[0]);
				Assert.Equal(7, command.UserId);
				Assert.Equal(42, command.ReminderId);
				Assert.Single(entries);
				events.Add("mark-published");
				marked.SetResult();
				return new ValueTask<PublishReminderResult>(new PublishReminderResult(true));
			}
		);
		using var services = new ServiceCollection().AddSingleton(sender).BuildServiceProvider();
		using var service = new TelegramReminderPublisherService(
			services,
			publisher,
			streamSettings,
			NullLogger<TelegramReminderPublisherService>.Instance
		);
		var before = DateTimeOffset.UtcNow;
		await service.StartAsync(CancellationToken.None);
		try
		{
			await marked.Task.WaitAsync(TimeSpan.FromSeconds(5));
		}
		finally
		{
			await service.StopAsync(CancellationToken.None);
		}

		var fields = Assert
			.Single(entries)
			.ToDictionary(entry => entry.Name.ToString(), entry => entry.Value.ToString());
		Assert.Equal(
			["chat_id", "created_at", "notification_id", "payload", "type", "user_id"],
			fields.Keys.Order().ToArray()
		);
		Assert.Equal("reminder:42", fields["notification_id"]);
		Assert.Equal("reminder", fields["type"]);
		Assert.Equal("7", fields["user_id"]);
		Assert.Equal("987654321", fields["chat_id"]);
		Assert.InRange(DateTimeOffset.Parse(fields["created_at"]), before, DateTimeOffset.UtcNow);
		Assert.Equal(["mark-published"], events);
		using var payload = JsonDocument.Parse(fields["payload"]);
		var root = payload.RootElement;
		Assert.Equal(
			["instrumentId", "instrumentTicker", "remindAt", "reminderId", "text"],
			root.EnumerateObject().Select(property => property.Name).Order().ToArray()
		);
		Assert.Equal(42, root.GetProperty("reminderId").GetInt32());
		Assert.Equal(reminder.Text, root.GetProperty("text").GetString());
		Assert.Equal(reminder.RemindAt, root.GetProperty("remindAt").GetDateTime());
		if (hasInstrument)
		{
			Assert.Equal(123, root.GetProperty("instrumentId").GetInt32());
			Assert.Equal("AAPL", root.GetProperty("instrumentTicker").GetString());
			await File.WriteAllTextAsync(
				Path.Combine(AppContext.BaseDirectory, "reminder-stream-entry.json"),
				JsonSerializer.Serialize(fields)
			);
		}
		else
		{
			Assert.Equal(JsonValueKind.Null, root.GetProperty("instrumentId").ValueKind);
			Assert.Equal(JsonValueKind.Null, root.GetProperty("instrumentTicker").ValueKind);
		}
	}

	[Fact]
	public async Task DoesNotMarkPublishedWhenPublishingFails()
	{
		var attempted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
		var commands = new List<MarkReminderPublishedCommand>();
		var sender = InterfaceDouble.Create<ISender>(
			(method, args) =>
			{
				if (args[0] is MarkReminderPublishedCommand command)
				{
					commands.Add(command);
					return new ValueTask<PublishReminderResult>(new PublishReminderResult(true));
				}
				Assert.IsType<ListDueRemindersQuery>(args[0]);
				return new ValueTask<IReadOnlyList<DueReminderResult>>([
					new DueReminderResult(
						42,
						"Buy",
						DateTime.UtcNow,
						new InstrumentBriefResult(123, "AAPL", null),
						7,
						"987654321"
					),
				]);
			}
		);
		var publisher = InterfaceDouble.Create<INotificationPublisher>(
			(method, args) =>
			{
				Assert.IsType<NotificationMessage>(args[0]);
				attempted.SetResult();
				return Task.FromException(new IOException("Redis unavailable"));
			}
		);
		using var services = new ServiceCollection().AddSingleton(sender).BuildServiceProvider();
		var options = Options.Create(new NotificationStreamSettings { ReminderPublishIntervalSeconds = 60 });
		using var service = new TelegramReminderPublisherService(
			services,
			publisher,
			options,
			NullLogger<TelegramReminderPublisherService>.Instance
		);
		await service.StartAsync(CancellationToken.None);
		try
		{
			await attempted.Task.WaitAsync(TimeSpan.FromSeconds(5));
		}
		finally
		{
			await service.StopAsync(CancellationToken.None);
		}
		Assert.Empty(commands);
	}
}
