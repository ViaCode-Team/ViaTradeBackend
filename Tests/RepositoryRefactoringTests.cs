using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using Ardalis.Specification.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using ViaTrade.Application.Common.Exceptions;
using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Instruments.Common;
using ViaTrade.Application.Instruments.GetBySymbol;
using ViaTrade.Application.Instruments.GetPage;
using ViaTrade.Application.Notes.Common;
using ViaTrade.Application.Notes.DeleteInstrument;
using ViaTrade.Application.Notes.Get;
using ViaTrade.Application.Notes.GetInstrument;
using ViaTrade.Application.Notes.GetPage;
using ViaTrade.Application.Notes.GetStrategy;
using ViaTrade.Application.Notes.UpsertInstrument;
using ViaTrade.Application.Reminders.Common;
using ViaTrade.Application.Reminders.Get;
using ViaTrade.Application.Reminders.GetPage;
using ViaTrade.Application.Reminders.ListDue;
using ViaTrade.Application.Signals.Common;
using ViaTrade.Application.Strategies.Common;
using ViaTrade.Application.Strategies.Get;
using ViaTrade.Application.Strategies.GetByInstrumentPage;
using ViaTrade.Application.Strategies.GetInstrumentsPage;
using ViaTrade.Application.Strategies.GetPage;
using ViaTrade.Application.Strategies.SetSubscription;
using ViaTrade.Application.Trades.Common;
using ViaTrade.Application.Trades.Get;
using ViaTrade.Application.Trades.GetPage;
using ViaTrade.Application.Users.GetCurrent;
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
	public async Task SingleNoteQueriesProjectNullableTargetsAndPreserveOwnership()
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
					Text = "Instrument",
				},
				1
			),
			WithId(
				context,
				new Note
				{
					UserId = 1,
					StrategyId = 1,
					Text = "Strategy",
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
		var repository = new ReadEfRepository<Note>(context);
		var handler = new GetNoteHandler(repository);
		var instrumentNote = await handler.HandleAsync(new GetNoteQuery(1, 1));
		Assert.Equal("GAZP", instrumentNote.Instrument!.Symbol);
		Assert.Null(instrumentNote.Strategy);
		Assert.DoesNotContain("CreatedAt", Assert.Single(database.Commands.Reads));
		var selectedColumns = database.Commands.Reads[0].Split("FROM", 2)[0];
		Assert.DoesNotContain("IsActive", selectedColumns);
		Assert.DoesNotContain("Accuracy", database.Commands.Reads[0]);
		var strategyNote = await handler.HandleAsync(new GetNoteQuery(1, 2));
		Assert.Null(strategyNote.Instrument);
		Assert.Equal("TrendFollowingStrategy", strategyNote.Strategy!.Name);
		await Assert.ThrowsAsync<NotFoundException>(() => handler.HandleAsync(new GetNoteQuery(2, 1)));
		var instrumentHandler = new GetInstrumentNoteHandler(new ReadEfRepository<Instrument>(context), repository);
		Assert.Equal(1, (await instrumentHandler.HandleAsync(new GetInstrumentNoteQuery(1, 1))).Id);
		var strategyHandler = new GetStrategyNoteHandler(new ReadEfRepository<Strategy>(context), repository);
		Assert.Equal(2, (await strategyHandler.HandleAsync(new GetStrategyNoteQuery(1, 1))).Id);
		await Assert.ThrowsAsync<NotFoundException>(() =>
			instrumentHandler.HandleAsync(new GetInstrumentNoteQuery(1, 100))
		);
		await Assert.ThrowsAsync<NotFoundException>(() =>
			strategyHandler.HandleAsync(new GetStrategyNoteQuery(1, 100))
		);
		await Assert.ThrowsAsync<NotFoundException>(() => strategyHandler.HandleAsync(new GetStrategyNoteQuery(2, 1)));
	}

	[Fact]
	public async Task NoteProjectionPreservesTheNoteWhenItsStrategyIsFilteredOut()
	{
		await using var database = await TestDatabase.CreateAsync();
		var context = database.Context;
		context.Strategies.Add(WithId(
			context,
			new Strategy { Name = "Inactive", DisplayName = "Inactive", IsActive = false },
			100
		));
		context.Notes.Add(WithId(context, new Note { UserId = 1, StrategyId = 100, Text = "Hidden strategy" }, 1));
		await database.SaveAsync();
		var repository = new ReadEfRepository<Note>(context);

		var note = await new GetNoteHandler(repository).HandleAsync(new GetNoteQuery(1, 1));
		Assert.Equal("Hidden strategy", note.Text);
		Assert.Null(note.Instrument);
		Assert.Null(note.Strategy);

		var page = await new GetNotesPageHandler(repository).HandleAsync(
			new GetNotesPageQuery(1, new NoteFilter(null), new NoteSearch(), new PageOptions())
		);
		Assert.Equal(1, page.TotalCount);
		Assert.Equal(note, Assert.Single(page.Items));
		Assert.Empty(context.ChangeTracker.Entries());
	}

	[Fact]
	public async Task SingleReminderQueryProjectsOnlyResponseFieldsAndPreservesOwnership()
	{
		await using var database = await TestDatabase.CreateAsync();
		var context = database.Context;
		context.Reminders.AddRange(CreateReminder(context, 1, 1), CreateReminder(context, 2, 2));
		await database.SaveAsync();
		var handler = new GetReminderHandler(new ReadEfRepository<Reminder>(context));
		var reminder = await handler.HandleAsync(new GetReminderQuery(1, 1));
		Assert.Equal("GAZP", reminder.Instrument!.Symbol);
		Assert.Null(reminder.DeliveredAt);
		Assert.Empty(reminder.TelegramId);
		var sql = Assert.Single(database.Commands.Reads);
		Assert.DoesNotContain("PublishedAt", sql);
		Assert.DoesNotContain("CreatedAt", sql);
		Assert.DoesNotContain("TelegramId", sql);
		await Assert.ThrowsAsync<NotFoundException>(() => handler.HandleAsync(new GetReminderQuery(2, 1)));
		await Assert.ThrowsAsync<NotFoundException>(() => handler.HandleAsync(new GetReminderQuery(1, 100)));
	}

	[Fact]
	public async Task InstrumentLookupUsesRequestedSymbolAndGenericPaging()
	{
		await using var database = await TestDatabase.CreateAsync();
		await database
			.Context.Instruments.Where(instrument => instrument.Symbol == "GMKN")
			.ExecuteUpdateAsync(setters => setters.SetProperty(instrument => instrument.Description, (string?)null));
		var getInstrumentBySymbolHandler = new GetInstrumentBySymbolHandler(
			new ReadEfRepository<Instrument>(database.Context)
		);
		var getInstrumentsPageHandler = new GetInstrumentsPageHandler(
			new ReadEfRepository<Instrument>(database.Context)
		);
		Assert.Equal(
			"GMKN",
			(await getInstrumentBySymbolHandler.HandleAsync(new GetInstrumentBySymbolQuery("GMKN"), default)).Symbol
		);
		await Assert.ThrowsAsync<NotFoundException>(() =>
			getInstrumentBySymbolHandler.HandleAsync(new GetInstrumentBySymbolQuery("Missing"), default)
		);
		database.Commands.Reads.Clear();
		var page = await getInstrumentsPageHandler.HandleAsync(
			new GetInstrumentsPageQuery(
				new InstrumentFilter("GMKN"),
				new InstrumentSearch { SearchText = "GMKN" },
				new PageOptions(),
				new InstrumentSort()
			),
			default
		);
		Assert.Equal("GMKN", Assert.Single(page.Items).Symbol);
		Assert.Equal(1, page.TotalCount);
		Assert.DoesNotContain("COUNT", Assert.Single(database.Commands.Reads), StringComparison.OrdinalIgnoreCase);
		database.Commands.Reads.Clear();
		var second = await getInstrumentsPageHandler.HandleAsync(
			new GetInstrumentsPageQuery(
				new InstrumentFilter("GMKN"),
				new InstrumentSearch(),
				new PageOptions { Page = 2 },
				new InstrumentSort()
			),
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
		var getCurrentUserHandler = new GetCurrentUserHandler(new ReadEfRepository<User>(database.Context));
		var user = await getCurrentUserHandler.HandleAsync(new GetCurrentUserQuery(1), default);
		Assert.Equal("one", user.Login);
		Assert.DoesNotContain("PasswordHash", Assert.Single(database.Commands.Reads));
		await Assert.ThrowsAsync<NotFoundException>(() =>
			getCurrentUserHandler.HandleAsync(new GetCurrentUserQuery(100), default)
		);
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
		var getStrategiesPageHandler = new GetStrategiesPageHandler(new ReadEfRepository<Strategy>(context));
		var getStrategyHandler = new GetStrategyHandler(new ReadEfRepository<Strategy>(context));
		var getStrategyInstrumentsPageHandler = new GetStrategyInstrumentsPageHandler(
			new ReadEfRepository<Strategy>(context),
			new ReadEfRepository<UserStrategyInstrument>(context)
		);
		Assert.True((await getStrategyHandler.HandleAsync(new GetStrategyQuery(1, 1), default)).IsSubscribed);
		Assert.False((await getStrategyHandler.HandleAsync(new GetStrategyQuery(2, 1), default)).IsSubscribed);
		var page = await getStrategiesPageHandler.HandleAsync(
			new GetStrategiesPageQuery(
				1,
				new StrategyFilter(null),
				new StrategySearch(),
				new StrategySort(),
				new PageOptions()
			),
			default
		);
		Assert.Equal(2, page.TotalCount);
		Assert.Single(page.Items, item => item.IsSubscribed);
		database.Commands.Reads.Clear();
		var second = await getStrategiesPageHandler.HandleAsync(
			new GetStrategiesPageQuery(
				1,
				new StrategyFilter("Test"),
				new StrategySearch(),
				new StrategySort(),
				new PageOptions { Page = 2 }
			),
			default
		);
		Assert.Empty(second.Items);
		Assert.Equal(1, second.TotalCount);
		Assert.Contains("COUNT", Assert.Single(database.Commands.Reads), StringComparison.OrdinalIgnoreCase);
		var instruments = await getStrategyInstrumentsPageHandler.HandleAsync(
			new GetStrategyInstrumentsPageQuery(
				1,
				1,
				new StrategyInstrumentFilter(null),
				new InstrumentSort(),
				new PageOptions()
			),
			default
		);
		Assert.Equal("GAZP", Assert.Single(instruments.Items).Symbol);
		await Assert.ThrowsAsync<NotFoundException>(() =>
			getStrategyInstrumentsPageHandler.HandleAsync(
				new GetStrategyInstrumentsPageQuery(
					1,
					100,
					new StrategyInstrumentFilter(null),
					new InstrumentSort(),
					new PageOptions()
				),
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
		var getStrategiesPageHandler = new GetStrategiesPageHandler(new ReadEfRepository<Strategy>(context));

		var page = await getStrategiesPageHandler.HandleAsync(
			new GetStrategiesPageQuery(
				1,
				new StrategyFilter(name),
				new StrategySearch { SearchText = searchText },
				new StrategySort(),
				new PageOptions()
			),
			default
		);

		if (expectedId == 0)
		{
			Assert.Empty(page.Items);
			Assert.Equal(0, page.TotalCount);
		}
		else
		{
			Assert.Equal(expectedId, Assert.Single(page.Items).Id);
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
		var getRemindersPageHandler = new GetRemindersPageHandler(repository);
		var complete = await getRemindersPageHandler.HandleAsync(
			new GetRemindersPageQuery(
				1,
				new ReminderFilter(null),
				new ReminderSearch(),
				new PageOptions { PageSize = 3 },
				new ReminderSort()
			),
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
		var first = await repository.GetPageAsync(
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
		var second = await repository.GetPageAsync(
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
		var beyond = await repository.GetPageAsync(
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

		var result = await repository.GetPageAsync(
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

		var result = await repository.GetPageAsync(specification, default);

		Assert.Empty(result.Items);
		Assert.Equal(0, result.TotalCount);
		Assert.Equal(0, result.TotalPages);
		Assert.Single(database.Commands.Reads);
	}

	[Theory]
	[InlineData(1, false)]
	[InlineData(1, true)]
	[InlineData(2, false)]
	[InlineData(2, true)]
	public async Task CancelledPageDoesNotSendDatabaseQueries(int page, bool project)
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

		if (project)
			await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
				repository.GetPageAsync(specification, instrument => instrument.Symbol, cancellation.Token)
			);
		else
			await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
				repository.GetPageAsync(specification, cancellation.Token)
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
		var page = PageResult<int>.FromList(new[] { 1, 2, 3 }, 2, 2);
		Assert.Equal(3, Assert.Single(page.Items));
		Assert.Equal(3, page.TotalCount);
		Assert.Equal(2, page.TotalPages);
		var beyond = PageResult<int>.FromList(new[] { 1, 2, 3 }, PageOptions.MaxPage, PageOptions.MaxPageSize);
		Assert.Empty(beyond.Items);
		Assert.Equal(3, beyond.TotalCount);
		Assert.Equal(0, PageResult<int>.FromList(Array.Empty<int>(), 2, 2).TotalPages);
		Assert.Equal(int.MaxValue, new PageResult<int>([], int.MaxValue, 1, 1).TotalPages);
		Assert.Equal(21_474_837, new PageResult<int>([], int.MaxValue, 1, 100).TotalPages);
	}

	[Theory]
	[InlineData(0, 2)]
	[InlineData(-1, 2)]
	[InlineData(1, 0)]
	[InlineData(1, -1)]
	public void InMemoryPaginationRejectsNonPositiveBounds(int page, int pageSize)
	{
		Assert.Throws<ArgumentOutOfRangeException>(() => PageResult<int>.FromList([1, 2, 3], page, pageSize));
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
		var getNotesPageHandler = new GetNotesPageHandler(repository);
		var page = await getNotesPageHandler.HandleAsync(
			new GetNotesPageQuery(1, new NoteFilter(null), new NoteSearch(), new PageOptions()),
			default
		);
		Assert.Equal(2, page.TotalCount);
		Assert.Equal("GAZP", page.Items[0].Instrument!.Symbol);
		Assert.Null(page.Items[0].Strategy);
		Assert.Equal("TrendFollowingStrategy", page.Items[1].Strategy!.Name);
		Assert.Null(page.Items[1].Instrument);
		var pageSql = Assert.Single(database.Commands.Reads);
		Assert.DoesNotContain("CreatedAt", pageSql);
		Assert.DoesNotContain("IsActive", pageSql.Split("FROM", 2)[0]);
		Assert.DoesNotContain("Accuracy", pageSql);
		Assert.Empty(context.ChangeTracker.Entries());

		var secondPage = await getNotesPageHandler.HandleAsync(
			new GetNotesPageQuery(
				1,
				new NoteFilter(null),
				new NoteSearch(),
				new PageOptions { Page = 2, PageSize = 1 }
			),
			default
		);
		Assert.Equal(2, secondPage.TotalCount);
		var secondPageNote = Assert.Single(secondPage.Items);
		Assert.Equal(2, secondPageNote.Id);
		Assert.Null(secondPageNote.Instrument);
		Assert.Equal("TrendFollowingStrategy", secondPageNote.Strategy!.Name);

		var instrumentSearch = await getNotesPageHandler.HandleAsync(
			new GetNotesPageQuery(1, new NoteFilter(null), new NoteSearch { SearchText = "GAZP" }, new PageOptions()),
			default
		);
		Assert.Equal(1, Assert.Single(instrumentSearch.Items).Id);
		var strategySearch = await getNotesPageHandler.HandleAsync(
			new GetNotesPageQuery(
				1,
				new NoteFilter(null),
				new NoteSearch { SearchText = "TrendFollowing" },
				new PageOptions()
			),
			default
		);
		Assert.Equal(2, Assert.Single(strategySearch.Items).Id);
		var otherUserHandler = new GetNoteHandler(repository);
		await Assert.ThrowsAsync<NotFoundException>(() => otherUserHandler.HandleAsync(new GetNoteQuery(2, 1)));

		var deleteInstrumentNoteHandler = new DeleteInstrumentNoteHandler(repository);
		var upsertInstrumentNoteHandler = new UpsertInstrumentNoteHandler(
			repository,
			operations,
			new EfUnitOfWork(context)
		);
		await upsertInstrumentNoteHandler.HandleAsync(new UpsertInstrumentNoteCommand(1, 1, "Updated"), default);
		Assert.Equal(
			"Other user",
			await repository.FirstOrDefaultAsync(note => note.UserId == 2, note => note.Text, default)
		);
		await deleteInstrumentNoteHandler.HandleAsync(new DeleteInstrumentNoteCommand(1, 1), default);
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
		var page = await links.GetPageAsync(
			specification,
			StrategySubscriptionResult.LinkProjection(1),
			default
		);
		Assert.Equal(new[] { 2, 1 }, page.Items.Select(item => item.Id));
		Assert.False(page.Items[0].IsSubscribed);
		Assert.True(page.Items[1].IsSubscribed);

		var setStrategySubscriptionHandler = new SetStrategySubscriptionHandler(
			subscriptions,
			new ReadEfRepository<Strategy>(context),
			new EfUnitOfWork(context)
		);
		await setStrategySubscriptionHandler.HandleAsync(new SetStrategySubscriptionCommand(1, 1, false), default);
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
		var listDueRemindersHandler = new ListDueRemindersHandler(repository);
		await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
			listDueRemindersHandler.HandleAsync(new ListDueRemindersQuery(0), default)
		);
	}

	[Fact]
	public async Task TradeProjectionsAndBulkChangesPreserveOwnershipAndComputedIncome()
	{
		await using var database = await TestDatabase.CreateAsync();
		var context = database.Context;
		context.Trades.AddRange(CreateTrade(context, 1, 1), CreateTrade(context, 2, 2));
		await database.SaveAsync();
		var repository = new EfRepository<Trade>(context);
		var getTradeHandler = new GetTradeHandler(repository);
		var getTradesPageHandler = new GetTradesPageHandler(repository);
		await Assert.ThrowsAsync<NotFoundException>(() =>
			getTradeHandler.HandleAsync(new GetTradeQuery(2, 1), default)
		);
		var projection = await getTradeHandler.HandleAsync(new GetTradeQuery(1, 1), default);
		Assert.Equal("GAZP", projection.Instrument!.Symbol);
		Assert.Equal(10, projection.NetIncome);
		var request = new TradeInput
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
		Assert.Equal(20, (await getTradeHandler.HandleAsync(new GetTradeQuery(1, 1), default)).NetIncome);
		var page = await getTradesPageHandler.HandleAsync(
			new GetTradesPageQuery(
				1,
				new TradeFilter(null, null, null, null, null),
				new TradeSearch(),
				new PageOptions()
			),
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
		var openPage = await getTradesPageHandler.HandleAsync(
			new GetTradesPageQuery(
				1,
				new TradeFilter(null, TradeStatus.Open, null, null, null),
				new TradeSearch { SearchText = "100" },
				new PageOptions()
			),
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
		var first = await repository.GetPageAsync(specification, instrument => instrument.Symbol, default);
		var second = await repository.GetPageAsync(secondSpecification, instrument => instrument.Symbol, default);
		Assert.Equal(new[] { "GAZP" }, first.Items);
		Assert.Equal(new[] { "GMKN" }, second.Items);
		Assert.Equal(2, first.TotalCount);
		Assert.Equal(2, second.TotalCount);
		Assert.Equal(2, await repository.CountAsync(specification, default));
		Assert.Equal(-1, specification.Skip);
		Assert.Equal(-1, specification.Take);
		Assert.Equal(1, specification.Page);
		Assert.Equal(1, specification.PageSize);
		var instruments = await repository.ListAsync(specification, default);
		Assert.Equal(new[] { "GAZP", "GMKN" }, instruments.Select(instrument => instrument.Symbol));

		var page = await repository.GetPageAsync(specification, instrument => instrument.Symbol, default);
		Assert.Equal(new[] { "GAZP" }, page.Items);
		Assert.Equal(2, page.TotalCount);
		Assert.Equal(-1, specification.Skip);
		Assert.Equal(-1, specification.Take);
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
		var tradeSql = evaluator.GetQuery(context.Trades, trade)
			.Skip(trade.Offset)
			.Take(trade.PageSize)
			.Select(TradeResult.Projection)
			.ToQueryString();
		Assert.Contains("UserId", tradeSql);
		Assert.Contains("IS NULL", tradeSql);
		Assert.Contains("ORDER BY", tradeSql);
		Assert.Contains("Instrument", tradeSql);
		Assert.Contains("LIMIT", tradeSql);
		Assert.Contains("OFFSET", tradeSql);
		var note = new NotesPageSpecification(1, new NoteFilter(null), new NoteSearch(), new PageOptions());
		var noteSql = evaluator.GetQuery(context.Notes, note).Select(NoteResult.Projection).ToQueryString();
		Assert.Contains("LEFT JOIN", noteSql);
		Assert.DoesNotContain("CreatedAt", noteSql);
		Assert.DoesNotContain("IsActive", noteSql.Split("FROM", 2)[0]);
		Assert.DoesNotContain("Accuracy", noteSql);
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
		var linkedSql = evaluator.GetQuery(context.UserStrategyInstruments, linkedInstruments)
			.Select(InstrumentResult.LinkProjection)
			.ToQueryString();
		Assert.Contains("UserId", linkedSql);
		Assert.Contains("StrategyId", linkedSql);
		var strategies = new StrategiesPageSpecification(
			new StrategyFilter("Test"),
			new StrategySearch { SearchText = "Test" },
			new PageOptions(),
			new StrategySort()
		);
		var strategiesSql = evaluator.GetQuery(context.Strategies, strategies)
			.Skip(strategies.Offset)
			.Take(strategies.PageSize)
			.Select(StrategySubscriptionResult.Projection(1))
			.ToQueryString();
		Assert.Contains("SignalFrequency", strategiesSql);
		Assert.Contains("InvestmentHorizon", strategiesSql);
		Assert.Contains("EXISTS", strategiesSql);
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
