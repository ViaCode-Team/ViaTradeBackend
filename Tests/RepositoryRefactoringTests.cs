using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using Ardalis.Specification.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using ViaTrade.Application.Common.Exceptions;
using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Common.Queries;
using ViaTrade.Application.Common.Specifications;
using ViaTrade.Application.Instruments;
using ViaTrade.Application.Instruments.Models;
using ViaTrade.Application.Instruments.Specifications;
using ViaTrade.Application.Notes;
using ViaTrade.Application.Notes.Models;
using ViaTrade.Application.Notes.Specifications;
using ViaTrade.Application.Reminders;
using ViaTrade.Application.Reminders.Models;
using ViaTrade.Application.Reminders.Specifications;
using ViaTrade.Application.Strategies;
using ViaTrade.Application.Strategies.Models;
using ViaTrade.Application.Strategies.Specifications;
using ViaTrade.Application.Trades;
using ViaTrade.Application.Trades.Models;
using ViaTrade.Application.Trades.Specifications;
using ViaTrade.Application.Users;
using ViaTrade.Domain.Entities;
using ViaTrade.Domain.Entities.Abstractions;
using ViaTrade.Domain.Enums;
using ViaTrade.Infrastructure.DataBase;
using ViaTrade.Infrastructure.DataBase.Repositories;
using ViaTrade.Infrastructure.DataBase.Repositories.Generic;
using Xunit;

namespace ViaTrade.Tests;

public sealed class RepositoryRefactoringTests
{
	[Fact]
	public async Task InstrumentLookupUsesRequestedSymbolAndGenericPaging()
	{
		await using var database = await TestDatabase.CreateAsync();
		await database
			.Context.Instruments.Where(instrument => instrument.Symbol == "GMKN")
			.ExecuteUpdateAsync(setters => setters.SetProperty(instrument => instrument.Description, (string?)null));
		var service = new InstrumentQueryService(null!, new ReadEfRepository<Instrument>(database.Context));
		Assert.Equal("GMKN", (await service.GetBySymbolAsync("GMKN", default)).Symbol);
		await Assert.ThrowsAsync<NotFoundException>(() => service.GetBySymbolAsync("Missing", default));
		database.Commands.Reads.Clear();
		var page = await service.GetPageAsync(
			new InstrumentFilter("GMKN"),
			new InstrumentSearch { SearchText = "GMKN" },
			new PageOptions(),
			new InstrumentSort(),
			default
		);
		Assert.Equal("GMKN", Assert.Single(page.Items).Symbol);
		Assert.Equal(1, page.TotalCount);
		Assert.DoesNotContain("COUNT", Assert.Single(database.Commands.Reads), StringComparison.OrdinalIgnoreCase);
		database.Commands.Reads.Clear();
		var second = await service.GetPageAsync(
			new InstrumentFilter("GMKN"),
			new InstrumentSearch(),
			new PageOptions { Page = 2 },
			new InstrumentSort(),
			default
		);
		Assert.Equal(1, second.TotalCount);
		Assert.Empty(second.Items);
		Assert.Contains("COUNT", Assert.Single(database.Commands.Reads), StringComparison.OrdinalIgnoreCase);
	}

	[Fact]
	public async Task UserProjectionDoesNotSelectPasswordHash()
	{
		await using var database = await TestDatabase.CreateAsync();
		var service = new UserQueryService(new ReadEfRepository<User>(database.Context), null!);
		var user = await service.GetCurrentUserAsync(1, default);
		Assert.Equal("one", user.Login);
		Assert.DoesNotContain("PasswordHash", Assert.Single(database.Commands.Reads));
		await Assert.ThrowsAsync<NotFoundException>(() => service.GetCurrentUserAsync(100, default));
	}

	[Fact]
	public async Task StrategyQueriesPreserveSubscriptionStateAndUserInstrumentLinks()
	{
		await using var database = await TestDatabase.CreateAsync();
		var context = database.Context;
		context.UserStrategies.Add(new UserStrategy { UserId = 1, StrategyId = 1 });
		context.UserStrategyInstruments.AddRange(
			new UserStrategyInstrument
			{
				UserId = 1,
				StrategyId = 1,
				InstrumentId = 1,
			},
			new UserStrategyInstrument
			{
				UserId = 2,
				StrategyId = 1,
				InstrumentId = 2,
			}
		);
		await database.SaveAsync();
		var service = new StrategyQueryService(
			new ReadEfRepository<Instrument>(context),
			new ReadEfRepository<Strategy>(context),
			new ReadEfRepository<UserStrategyInstrument>(context),
			new StrategyEfRepository(context)
		);
		Assert.True((await service.GetAsync(1, 1, default)).IsSubscribed);
		Assert.False((await service.GetAsync(2, 1, default)).IsSubscribed);
		var page = await service.GetPageAsync(
			1,
			new StrategyFilter(null),
			new StrategySearch(),
			new StrategySort(),
			new PageOptions(),
			default
		);
		Assert.Equal(2, page.TotalCount);
		Assert.Single(page.Items, item => item.IsSubscribed);
		database.Commands.Reads.Clear();
		var second = await service.GetPageAsync(
			1,
			new StrategyFilter("Test"),
			new StrategySearch(),
			new StrategySort(),
			new PageOptions { Page = 2 },
			default
		);
		Assert.Empty(second.Items);
		Assert.Equal(1, second.TotalCount);
		Assert.Contains("COUNT", Assert.Single(database.Commands.Reads), StringComparison.OrdinalIgnoreCase);
		var instruments = await service.GetInstrumentsByStrategyPageAsync(
			1,
			1,
			new StrategyInstrumentFilter(null),
			new InstrumentSort(),
			new PageOptions(),
			default
		);
		Assert.Equal("GAZP", Assert.Single(instruments.Items).Symbol);
		await Assert.ThrowsAsync<NotFoundException>(() =>
			service.GetInstrumentsByStrategyPageAsync(
				1,
				100,
				new StrategyInstrumentFilter(null),
				new InstrumentSort(),
				new PageOptions(),
				default
			)
		);
	}

	[Theory]
	[InlineData(null, "OnlyName", 100)]
	[InlineData("OnlyName", "OnlyName", 100)]
	[InlineData("OnlyName", "Only", 100)]
	[InlineData("OnlyName", "Label", 100)]
	[InlineData("OnlyName", "Missing", 0)]
	[InlineData("Test", "OnlyName", 0)]
	[InlineData(null, "weekly", 101)]
	[InlineData(null, "short", 101)]
	public async Task StrategySearchWorksWithExactFiltersAndNullOptionalFields(
		string? name,
		string searchText,
		int expectedId
	)
	{
		await using var database = await TestDatabase.CreateAsync();
		var context = database.Context;
		context.Strategies.AddRange(
			WithId(
				context,
				new Strategy
				{
					Name = "OnlyName",
					DisplayName = "Label",
					IsActive = true,
				},
				100
			),
			WithId(
				context,
				new Strategy
				{
					Name = "OtherName",
					DisplayName = "OtherLabel",
					IsActive = true,
					SignalFrequency = "weekly",
					InvestmentHorizon = "short",
				},
				101
			)
		);
		await database.SaveAsync();
		var service = new StrategyQueryService(null!, new ReadEfRepository<Strategy>(context), null!, null!);

		var page = await service.GetPageAsync(
			1,
			new StrategyFilter(name),
			new StrategySearch { SearchText = searchText },
			new StrategySort(),
			new PageOptions(),
			default
		);

		if (expectedId == 0)
		{
			Assert.Empty(page.Items);
			Assert.Equal(0, page.TotalCount);
		}
		else
		{
			Assert.Equal(expectedId, Assert.Single(page.Items).Strategy.Id);
			Assert.Equal(1, page.TotalCount);
		}
	}

	[Fact]
	public async Task PagingPreservesTotalsStableOrderAndFirstPageOptimization()
	{
		await using var database = await TestDatabase.CreateAsync();
		var context = database.Context;
		context.Reminders.AddRange(
			CreateReminder(context, 3, 1),
			CreateReminder(context, 1, 1),
			CreateReminder(context, 2, 1),
			CreateReminder(context, 4, 2)
		);
		await database.SaveAsync();
		var repository = new ReadEfRepository<Reminder>(context);
		var service = new ReminderQueryService(new ReadEfRepository<Instrument>(context), repository, null!);
		var complete = await service.GetPageAsync(
			1,
			new ReminderFilter(null),
			new ReminderSearch(),
			new PageOptions { PageSize = 3 },
			new ReminderSort(),
			default
		);
		Assert.Equal(3, complete.TotalCount);
		Assert.Equal(new[] { 1, 2, 3 }, complete.Items.Select(reminder => reminder.Id));
		Assert.Single(database.Commands.Reads);
		Assert.DoesNotContain("COUNT", database.Commands.Reads[0], StringComparison.OrdinalIgnoreCase);
		Assert.All(
			complete.Items,
			reminder =>
			{
				Assert.Equal("GAZP", reminder.Instrument!.Symbol);
				Assert.Equal(1, reminder.UserId);
				Assert.Equal(string.Empty, reminder.TelegramId);
			}
		);

		database.Commands.Reads.Clear();
		var firstSpecification = new RemindersPageSpecification(
			1,
			new ReminderFilter(null),
			new ReminderSearch(),
			new PageOptions { PageSize = 2 },
			new ReminderSort()
		);
		var first = await PageQuery.ExecuteAsync(
			repository,
			firstSpecification,
			reminder => new { reminder.Id, InstrumentTicker = reminder.Instrument!.Symbol },
			default
		);
		Assert.Equal(3, first.TotalCount);
		Assert.Equal(new[] { 1, 2 }, first.Items.Select(reminder => reminder.Id));
		Assert.Equal(2, database.Commands.Reads.Count);

		var secondSpecification = new RemindersPageSpecification(
			1,
			new ReminderFilter(null),
			new ReminderSearch(),
			new PageOptions { Page = 2, PageSize = 2 },
			new ReminderSort()
		);
		var second = await PageQuery.ExecuteAsync(
			repository,
			secondSpecification,
			reminder => new { reminder.Id, InstrumentTicker = reminder.Instrument!.Symbol },
			default
		);
		Assert.Equal(3, second.TotalCount);
		Assert.Equal(3, Assert.Single(second.Items).Id);
		database.Commands.Reads.Clear();
		var beyondSpecification = new RemindersPageSpecification(
			1,
			new ReminderFilter(null),
			new ReminderSearch(),
			new PageOptions { Page = 3, PageSize = 2 },
			new ReminderSort()
		);
		var beyond = await PageQuery.ExecuteAsync(
			repository,
			beyondSpecification,
			reminder => new { reminder.Id, InstrumentTicker = reminder.Instrument!.Symbol },
			default
		);
		Assert.Equal(3, beyond.TotalCount);
		Assert.Empty(beyond.Items);
		Assert.Contains("COUNT", Assert.Single(database.Commands.Reads), StringComparison.OrdinalIgnoreCase);
		Assert.Empty(context.ChangeTracker.Entries());
	}

	[Theory]
	[InlineData(1, 1, 1, 1, 2)]
	[InlineData(1, 3, 1, 3, 1)]
	[InlineData(1, PageOptions.MaxPageSize, 1, 3, 1)]
	[InlineData(2, 1, 2, 1, 2)]
	[InlineData(3, 1, 3, 1, 2)]
	[InlineData(4, 1, 0, 0, 1)]
	[InlineData(2, 3, 0, 0, 1)]
	[InlineData(PageOptions.MaxPage, PageOptions.MaxPageSize, 0, 0, 1)]
	public async Task PaginationHonorsBoundsAndSkipsKnownEmptyPages(
		int page,
		int pageSize,
		int firstId,
		int itemCount,
		int queryCount
	)
	{
		await using var database = await TestDatabase.CreateAsync();
		var context = database.Context;
		context.Reminders.AddRange(
			CreateReminder(context, 3, 1),
			CreateReminder(context, 1, 1),
			CreateReminder(context, 2, 1),
			CreateReminder(context, 4, 2)
		);
		await database.SaveAsync();
		var repository = new ReadEfRepository<Reminder>(context);
		var specification = new RemindersPageSpecification(
			1,
			new ReminderFilter(null),
			new ReminderSearch(),
			new PageOptions { Page = page, PageSize = pageSize },
			new ReminderSort()
		);

		var result = await PageQuery.ExecuteAsync(
			repository,
			specification,
			reminder => new { reminder.Id, InstrumentTicker = reminder.Instrument!.Symbol },
			default
		);

		Assert.Equal(3, result.TotalCount);
		Assert.Equal(page, result.Page);
		Assert.Equal(pageSize, result.PageSize);
		Assert.Equal(itemCount, result.Items.Count);
		Assert.Equal(queryCount, database.Commands.Reads.Count);
		if (itemCount > 0)
			Assert.Equal(firstId, result.Items[0].Id);
		else
			Assert.Contains("COUNT", Assert.Single(database.Commands.Reads), StringComparison.OrdinalIgnoreCase);
	}

	[Theory]
	[InlineData(1)]
	[InlineData(2)]
	[InlineData(PageOptions.MaxPage)]
	public async Task EmptyFilteredResultNeedsOnlyOneQuery(int page)
	{
		await using var database = await TestDatabase.CreateAsync();
		var repository = new ReadEfRepository<Instrument>(database.Context);
		var specification = new InstrumentsPageSpecification(
			new InstrumentFilter("Missing"),
			new InstrumentSearch(),
			new PageOptions { Page = page },
			new InstrumentSort()
		);

		var result = await PageQuery.ExecuteAsync(repository, specification, default);

		Assert.Empty(result.Items);
		Assert.Equal(0, result.TotalCount);
		Assert.Equal(0, result.TotalPages);
		Assert.Single(database.Commands.Reads);
	}

	[Theory]
	[InlineData(1)]
	[InlineData(2)]
	public async Task CancelledPageDoesNotSendDatabaseQueries(int page)
	{
		await using var database = await TestDatabase.CreateAsync();
		var repository = new ReadEfRepository<Instrument>(database.Context);
		var specification = new InstrumentsPageSpecification(
			new InstrumentFilter(null),
			new InstrumentSearch(),
			new PageOptions { Page = page },
			new InstrumentSort()
		);
		using var cancellation = new CancellationTokenSource();
		cancellation.Cancel();

		await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
			PageQuery.ExecuteAsync(repository, specification, cancellation.Token)
		);
		Assert.Empty(database.Commands.Reads);
	}

	[Theory]
	[InlineData(0, 20)]
	[InlineData(-1, 20)]
	[InlineData(PageOptions.MaxPage + 1, 20)]
	[InlineData(1, 0)]
	[InlineData(1, PageOptions.MaxPageSize + 1)]
	public void InvalidPaginationIsRejectedByInputModelValidation(int page, int pageSize)
	{
		var options = new PageOptions { Page = page, PageSize = pageSize };
		var errors = new List<ValidationResult>();
		Assert.False(Validator.TryValidateObject(options, new ValidationContext(options), errors, true));
		Assert.NotEmpty(errors);
	}

	[Fact]
	public void InMemoryPaginationAndPageTotalsHandleBoundaryValues()
	{
		var specification = new PageSpecification<int>(new PageOptions { Page = 2, PageSize = 2 });
		var page = PageQuery.FromList(new[] { 1, 2, 3 }, specification);
		Assert.Equal(3, Assert.Single(page.Items));
		Assert.Equal(3, page.TotalCount);
		Assert.Equal(2, page.TotalPages);
		var beyond = new PageSpecification<int>(
			new PageOptions { Page = PageOptions.MaxPage, PageSize = PageOptions.MaxPageSize }
		);
		Assert.Empty(PageQuery.FromList(new[] { 1, 2, 3 }, beyond).Items);
		Assert.Equal(3, PageQuery.FromList(new[] { 1, 2, 3 }, beyond).TotalCount);
		Assert.Equal(0, PageQuery.FromList(Array.Empty<int>(), specification).TotalPages);
		Assert.Equal(int.MaxValue, new PageResult<int>([], int.MaxValue, 1, 1).TotalPages);
		Assert.Equal(21_474_837, new PageResult<int>([], int.MaxValue, 1, 100).TotalPages);
	}

	[Fact]
	public async Task ProjectedSingleOrDefaultRejectsDuplicateMatches()
	{
		await using var database = await TestDatabase.CreateAsync();
		var repository = new ReadEfRepository<Instrument>(database.Context);
		await Assert.ThrowsAsync<InvalidOperationException>(() =>
			repository.SingleOrDefaultAsync(instrument => true, instrument => instrument.Symbol, default)
		);
	}

	[Fact]
	public async Task GenericReadsRespectGlobalFiltersAndUpdatesRequireExplicitAttachment()
	{
		await using var database = await TestDatabase.CreateAsync();
		var context = database.Context;
		context.Strategies.Add(
			WithId(
				context,
				new Strategy
				{
					Name = "Inactive",
					DisplayName = "Inactive",
					IsActive = false,
				},
				100
			)
		);
		await database.SaveAsync();
		var strategies = new ReadEfRepository<Strategy>(context);
		Assert.Null(await strategies.GetByIdAsync(100, default));

		var repository = new EfRepository<Instrument>(context);
		var instrument = await repository.FirstOrDefaultAsync(instrument => instrument.Id == 1, default);
		Assert.NotNull(instrument);
		Assert.Empty(context.ChangeTracker.Entries());
		instrument.Description = "Updated";
		repository.Update(instrument);
		await repository.SaveChangesAsync(default);
		context.ChangeTracker.Clear();
		Assert.Equal(
			"Updated",
			await repository.FirstOrDefaultAsync(
				instrument => instrument.Id == 1,
				instrument => instrument.Description,
				default
			)
		);
	}

	[Fact]
	public async Task NotesProjectBothTargetTypesAndBulkOperationsStayUserScoped()
	{
		await using var database = await TestDatabase.CreateAsync();
		var context = database.Context;
		context.Notes.AddRange(
			WithId(
				context,
				new Note
				{
					UserId = 1,
					InstrumentId = 1,
					Text = "Instrument note",
				},
				1
			),
			WithId(
				context,
				new Note
				{
					UserId = 1,
					StrategyId = 1,
					Text = "Strategy note",
				},
				2
			),
			WithId(
				context,
				new Note
				{
					UserId = 2,
					InstrumentId = 1,
					Text = "Other user",
				},
				3
			)
		);
		await database.SaveAsync();
		var repository = new EfRepository<Note>(context);
		var operations = new NoteEfRepository(context);
		var queryService = new NoteQueryService(
			new ReadEfRepository<Instrument>(context),
			new ReadEfRepository<Strategy>(context),
			repository,
			operations
		);
		var page = await queryService.GetPageAsync(
			1,
			new NoteFilter(null),
			new NoteSearch(),
			new PageOptions(),
			default
		);
		Assert.Equal(2, page.TotalCount);
		Assert.Equal("GAZP", page.Items[0].Instrument!.Symbol);
		Assert.Null(page.Items[0].Strategy);
		Assert.Equal("TrendFollowingStrategy", page.Items[1].Strategy!.Name);
		Assert.Null(page.Items[1].Instrument);
		var instrumentSearch = await queryService.GetPageAsync(
			1,
			new NoteFilter(null),
			new NoteSearch { SearchText = "GAZP" },
			new PageOptions(),
			default
		);
		Assert.Equal(1, Assert.Single(instrumentSearch.Items).Id);
		var strategySearch = await queryService.GetPageAsync(
			1,
			new NoteFilter(null),
			new NoteSearch { SearchText = "TrendFollowing" },
			new PageOptions(),
			default
		);
		Assert.Equal(2, Assert.Single(strategySearch.Items).Id);
		var otherUserSpecification = new NoteWithTargetsSpecification(2, note => note.Id == 1);
		Assert.Null(await repository.FirstOrDefaultAsync(otherUserSpecification, default));

		var commands = new NoteCommandService(repository, operations, new EfUnitOfWork(context));
		await commands.UpsertInstrumentAsync(1, 1, "Updated", default);
		Assert.Equal(
			"Other user",
			await repository.FirstOrDefaultAsync(note => note.UserId == 2, note => note.Text, default)
		);
		await commands.DeleteInstrumentAsync(1, 1, default);
		Assert.True(await repository.AnyAsync(note => note.Id == 3 && note.UserId == 2, default));
		Assert.Equal(1, await repository.CountAsync(note => note.UserId == 1, default));
	}

	[Fact]
	public async Task SubscriptionDeletionPreservesInstrumentConfigurationAndSignalIsolation()
	{
		await using var database = await TestDatabase.CreateAsync();
		var context = database.Context;
		context.UserStrategies.AddRange(
			new UserStrategy { UserId = 1, StrategyId = 1 },
			new UserStrategy { UserId = 2, StrategyId = 2 }
		);
		context.UserStrategyInstruments.AddRange(
			new UserStrategyInstrument
			{
				UserId = 1,
				StrategyId = 1,
				InstrumentId = 1,
			},
			new UserStrategyInstrument
			{
				UserId = 1,
				StrategyId = 2,
				InstrumentId = 1,
			},
			new UserStrategyInstrument
			{
				UserId = 2,
				StrategyId = 2,
				InstrumentId = 2,
			}
		);
		await database.SaveAsync();
		var links = new EfRepository<UserStrategyInstrument>(context);
		var subscriptions = new EfRepository<UserStrategy>(context);
		var sourcesSpecification = new SignalSourcesSpecification(1);
		var sources = await links.ListAsync(sourcesSpecification, default);
		Assert.Equal(1, Assert.Single(sources).StrategyId);
		Assert.Equal("GAZP", sources[0].Symbol);

		var specification = new InstrumentStrategiesPageSpecification(
			1,
			1,
			new StrategyFilter(null),
			new PageOptions(),
			new StrategySort { SortBy = [StrategySortField.AccuracyDesc, StrategySortField.NameAsc] }
		);
		var page = await PageQuery.ExecuteAsync(
			links,
			specification,
			link => new StrategySubscriptionDto(
				link.Strategy!,
				link.Strategy!.UserStrategies.Any(subscription => subscription.UserId == 1)
			),
			default
		);
		Assert.Equal(new[] { 2, 1 }, page.Items.Select(item => item.Strategy.Id));
		Assert.False(page.Items[0].IsSubscribed);
		Assert.True(page.Items[1].IsSubscribed);

		var service = new StrategyCommandService(
			links,
			subscriptions,
			new ReadEfRepository<Strategy>(context),
			new StrategyEfRepository(context),
			new EfUnitOfWork(context)
		);
		await service.SetSubscriptionAsync(1, 1, false, default);
		Assert.Equal(2, await links.CountAsync(link => link.UserId == 1, default));
		Assert.Empty(await links.ListAsync(sourcesSpecification, default));
		Assert.True(await subscriptions.AnyAsync(link => link.UserId == 2 && link.StrategyId == 2, default));
	}

	[Fact]
	public async Task ReminderUpdatesAreAtomicScopedAndDeliveryIsIdempotent()
	{
		await using var database = await TestDatabase.CreateAsync();
		var context = database.Context;
		context.Reminders.AddRange(
			CreateReminder(context, 1, 1),
			CreateReminder(context, 2, 2),
			CreateReminder(context, 3, 1, DateTime.UtcNow.AddDays(1))
		);
		await database.SaveAsync();
		var operations = new ReminderEfRepository(context);
		Assert.Equal(0, await operations.ExecuteUpdateForUserAsync(2, 1, "Wrong user", DateTime.UtcNow, default));
		Assert.Equal(0, await operations.ExecuteMarkPublishedAsync(2, 1, default));
		Assert.Equal(0, await operations.ExecuteMarkDeliveredForUserAsync(2, 1, default));
		Assert.Equal(0, await operations.ExecuteMarkDeliveredForUserAsync(1, 3, default));
		Assert.Equal(1, await operations.ExecuteMarkPublishedAsync(1, 1, default));
		Assert.Equal(0, await operations.ExecuteUpdateForUserAsync(1, 1, "Too late", DateTime.UtcNow, default));
		Assert.Equal(1, await operations.ExecuteMarkDeliveredForUserAsync(1, 1, default));
		var delivered = await context.Reminders.SingleAsync(reminder => reminder.Id == 1 && reminder.UserId == 1);
		Assert.Equal(1, await operations.ExecuteMarkDeliveredForUserAsync(1, 1, default));
		var repeated = await context.Reminders.SingleAsync(reminder => reminder.Id == 1 && reminder.UserId == 1);
		Assert.Equal(delivered.PublishedAt, repeated.PublishedAt);
		Assert.Equal(delivered.DeliveredAt, repeated.DeliveredAt);
		Assert.Equal("Reminder", repeated.Text);
		Assert.Null(
			(await context.Reminders.SingleAsync(reminder => reminder.Id == 2 && reminder.UserId == 2)).PublishedAt
		);
	}

	[Fact]
	public async Task DueReminderBatchHonorsLimitTimeAndTelegramAvailability()
	{
		await using var database = await TestDatabase.CreateAsync();
		var context = database.Context;
		var now = DateTime.UtcNow;
		context
			.Users.Where(user => user.Id == 2)
			.ExecuteUpdate(setters => setters.SetProperty(user => user.TelegramId, (string?)null));
		context.Reminders.AddRange(
			CreateReminder(context, 2, 1),
			CreateReminder(context, 1, 1),
			CreateReminder(context, 3, 2),
			CreateReminder(context, 4, 1, now.AddHours(1))
		);
		await database.SaveAsync();
		var repository = new ReadEfRepository<Reminder>(context);
		var specification = new DueRemindersSpecification(1, now);
		var batch = await repository.ListAsync(specification, default);
		Assert.Equal(1, Assert.Single(batch).Id);
		Assert.Equal("telegram-1", batch[0].TelegramId);
		var service = new ReminderQueryService(new ReadEfRepository<Instrument>(context), repository, null!);
		await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => service.ListDueBatchAsync(0, default));
	}

	[Fact]
	public async Task TradeProjectionsAndBulkChangesPreserveOwnershipAndComputedIncome()
	{
		await using var database = await TestDatabase.CreateAsync();
		var context = database.Context;
		context.Trades.AddRange(CreateTrade(context, 1, 1), CreateTrade(context, 2, 2));
		await database.SaveAsync();
		var repository = new EfRepository<Trade>(context);
		var service = new TradeQueryService(repository, new TradeEfRepository(context));
		await Assert.ThrowsAsync<NotFoundException>(() => service.GetAsync(2, 1, default));
		var projection = await service.GetAsync(1, 1, default);
		Assert.Equal("GAZP", projection.Instrument!.Symbol);
		Assert.Equal(10, projection.NetIncome);
		var request = new TradeInputDto
		{
			OpenedAt = projection.OpenedAt,
			ClosedAt = projection.ClosedAt,
			OpenPrice = 100,
			ClosePrice = 120,
			Quantity = 2,
			Signal = TradeSignal.BUY,
			TradeTypeId = 1,
			InstrumentId = 1,
		};
		var operations = new TradeEfRepository(context);
		Assert.Equal(0, await operations.ExecuteUpdateAsync(2, 1, request, 200, default));
		Assert.Equal(1, await operations.ExecuteUpdateAsync(1, 1, request, 200, default));
		Assert.Equal(20, (await service.GetAsync(1, 1, default)).NetIncome);
		var page = await service.GetPageAsync(
			1,
			new TradeFilter(null, null, null, null, null),
			new TradeSearch(),
			new PageOptions(),
			default
		);
		Assert.Equal(1, page.TotalCount);
		Assert.Equal(1, Assert.Single(page.Items).Id);
		Assert.Equal("GAZP", page.Items[0].Instrument!.Symbol);
		var openTrade = CreateTrade(context, 3, 1);
		openTrade.ClosedAt = null;
		openTrade.ClosePrice = null;
		context.Trades.Add(openTrade);
		await database.SaveAsync();
		var openPage = await service.GetPageAsync(
			1,
			new TradeFilter(null, TradeStatus.Open, null, null, null),
			new TradeSearch { SearchText = "100" },
			new PageOptions(),
			default
		);
		Assert.Equal(3, Assert.Single(openPage.Items).Id);
		Assert.Null(openPage.Items[0].ClosedAt);
		Assert.Null(openPage.Items[0].ClosePrice);
		Assert.Equal(0, await repository.ExecuteDeleteAsync(trade => trade.UserId == 2 && trade.Id == 1, default));
		Assert.Equal(1, await repository.ExecuteDeleteAsync(trade => trade.UserId == 1 && trade.Id == 1, default));
		Assert.True(await repository.AnyAsync(trade => trade.UserId == 2 && trade.Id == 2, default));
	}

	[Fact]
	public async Task PagingAndProjectionReuseFiltersWithoutMutatingTheSpecification()
	{
		await using var database = await TestDatabase.CreateAsync();
		var repository = new ReadEfRepository<Instrument>(database.Context);
		var pageOptions = new PageOptions { PageSize = 1 };
		var specification = new InstrumentsPageSpecification(
			new InstrumentFilter(null),
			new InstrumentSearch(),
			pageOptions,
			new InstrumentSort()
		);
		pageOptions.Page = 2;
		pageOptions.PageSize = 100;
		var secondSpecification = new InstrumentsPageSpecification(
			new InstrumentFilter(null),
			new InstrumentSearch(),
			new PageOptions { Page = 2, PageSize = 1 },
			new InstrumentSort()
		);
		var first = specification.ForPage(instrument => instrument.Symbol);
		var second = secondSpecification.ForPage(instrument => instrument.Symbol);
		Assert.Equal(new[] { "GAZP" }, await repository.ListAsync(first, default));
		Assert.Equal(new[] { "GMKN" }, await repository.ListAsync(second, default));
		Assert.Equal(2, await repository.CountAsync(first, default));
		Assert.Equal(-1, specification.Skip);
		Assert.Equal(-1, specification.Take);
		Assert.Equal(1, specification.Page);
		Assert.Equal(1, specification.PageSize);
		var projection = specification.Project(instrument => instrument.Symbol);
		Assert.Equal(new[] { "GAZP", "GMKN" }, await repository.ListAsync(projection, default));
	}

	[Fact]
	public void SpecificationsTranslateWithProductionMySqlProvider()
	{
		var options = new DbContextOptionsBuilder<AppDbContext>()
			.UseMySql(
				"Server=localhost;Database=translation_only;User=test;Password=test",
				new MySqlServerVersion(new Version(8, 0, 36))
			)
			.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
			.Options;
		using var context = new AppDbContext(options);
		var evaluator = SpecificationEvaluator.Default;
		var trade = new TradesPageSpecification(
			1,
			new TradeFilter(null, TradeStatus.Open, null, null, null),
			new TradeSearch { SearchText = "GAZP" },
			new PageOptions { Page = 2 }
		);
		var tradePage = trade.ForPage(trade => new
		{
			trade.Id,
			trade.UserId,
			trade.NetIncome,
			trade.Instrument!.Symbol,
		});
		var tradeSql = evaluator.GetQuery(context.Trades, tradePage).ToQueryString();
		Assert.Contains("UserId", tradeSql);
		Assert.Contains("IS NULL", tradeSql);
		Assert.Contains("ORDER BY", tradeSql);
		Assert.Contains("Instrument", tradeSql);
		Assert.Contains("LIMIT", tradeSql);
		Assert.Contains("OFFSET", tradeSql);
		var note = new NotesPageSpecification(1, new NoteFilter(null), new NoteSearch(), new PageOptions());
		var noteProjection = note.Project(note => new
		{
			note.Id,
			InstrumentTicker = note.Instrument!.Symbol,
			StrategyName = note.Strategy!.Name,
		});
		var noteSql = evaluator.GetQuery(context.Notes, noteProjection).ToQueryString();
		Assert.Contains("LEFT JOIN", noteSql);
		var signal = new SignalSourcesSpecification(1);
		var signalSql = evaluator.GetQuery(context.UserStrategyInstruments, signal).ToQueryString();
		Assert.Contains("EXISTS", signalSql);
		Assert.Contains("IsActive", signalSql);
		var dueReminders = new DueRemindersSpecification(10, DateTime.UtcNow);
		Assert.Contains("LIMIT", evaluator.GetQuery(context.Reminders, dueReminders).ToQueryString());
		var linkedInstruments = new StrategyInstrumentsPageSpecification(
			1,
			1,
			new StrategyInstrumentFilter(null),
			new PageOptions(),
			new InstrumentSort()
		);
		var linkedProjection = linkedInstruments.Project(link => new RelatedInstrumentDto(
			link.Instrument!.Id,
			link.Instrument.Symbol,
			link.Instrument.Description
		));
		var linkedSql = evaluator.GetQuery(context.UserStrategyInstruments, linkedProjection).ToQueryString();
		Assert.Contains("UserId", linkedSql);
		Assert.Contains("StrategyId", linkedSql);
		var strategies = new StrategiesPageSpecification(
			new StrategyFilter("Test"),
			new StrategySearch { SearchText = "Test" },
			new PageOptions(),
			new StrategySort()
		);
		var strategiesPage = strategies.ForPage(strategy => strategy.Name);
		var strategiesSql = evaluator.GetQuery(context.Strategies, strategiesPage).ToQueryString();
		Assert.Contains("SignalFrequency", strategiesSql);
		Assert.Contains("InvestmentHorizon", strategiesSql);
	}

	private static T WithId<T>(AppDbContext context, T entity, int id)
		where T : BaseEntity<int>
	{
		context.Entry(entity).Property(entity => entity.Id).CurrentValue = id;
		return entity;
	}

	private static Reminder CreateReminder(AppDbContext context, int id, int userId, DateTime? remindAt = null)
	{
		return WithId(
			context,
			new Reminder
			{
				UserId = userId,
				InstrumentId = 1,
				Text = "Reminder",
				RemindAt = remindAt ?? new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
			},
			id
		);
	}

	private static Trade CreateTrade(AppDbContext context, int id, int userId)
	{
		return WithId(
			context,
			new Trade
			{
				UserId = userId,
				InstrumentId = 1,
				TradeTypeId = 1,
				OpenedAt = DateTime.UtcNow.AddDays(-1),
				ClosedAt = DateTime.UtcNow,
				OpenPrice = 100,
				ClosePrice = 110,
				Quantity = 1,
				Signal = TradeSignal.BUY,
				TotalPrice = 100,
			},
			id
		);
	}

	private sealed class TestDatabase(SqliteConnection connection, AppDbContext context, CommandRecorder commands)
		: IAsyncDisposable
	{
		public AppDbContext Context { get; } = context;
		public CommandRecorder Commands { get; } = commands;

		public static async Task<TestDatabase> CreateAsync()
		{
			var connection = new SqliteConnection("Data Source=:memory:");
			await connection.OpenAsync();
			var commands = new CommandRecorder();
			var options = new DbContextOptionsBuilder<AppDbContext>()
				.UseSqlite(connection)
				.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
				.AddInterceptors(commands)
				.Options;
			var context = new AppDbContext(options);
			await context.Database.EnsureCreatedAsync();
			context.Users.AddRange(
				WithId(
					context,
					new User
					{
						Login = "one",
						PasswordHash = "test",
						RegisteredAt = DateTime.UtcNow,
						TelegramId = "telegram-1",
					},
					1
				),
				WithId(
					context,
					new User
					{
						Login = "two",
						PasswordHash = "test",
						RegisteredAt = DateTime.UtcNow,
						TelegramId = "telegram-2",
					},
					2
				)
			);
			var database = new TestDatabase(connection, context, commands);
			await database.SaveAsync();
			return database;
		}

		public async Task SaveAsync()
		{
			await Context.SaveChangesAsync();
			Context.ChangeTracker.Clear();
			Commands.Reads.Clear();
		}

		public async ValueTask DisposeAsync()
		{
			await Context.DisposeAsync();
			await connection.DisposeAsync();
		}
	}

	private sealed class CommandRecorder : DbCommandInterceptor
	{
		public List<string> Reads { get; } = [];

		public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
			DbCommand command,
			CommandEventData eventData,
			InterceptionResult<DbDataReader> result,
			CancellationToken cancellationToken = default
		)
		{
			Reads.Add(command.CommandText);
			return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
		}
	}
}
