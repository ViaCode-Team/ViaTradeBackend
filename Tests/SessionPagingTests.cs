using System.Reflection;
using ViaTrade.Application.Auth.Common;
using ViaTrade.Application.Auth.Common.Abstractions;
using ViaTrade.Application.Auth.GetSessionsPage;
using ViaTrade.Application.Common.Models;
using Xunit;

namespace ViaTrade.Tests;

public sealed class SessionPagingTests
{
	[Theory]
	[InlineData(1, 2, "newest")]
	[InlineData(2, 1, "oldest")]
	[InlineData(3, 0, null)]
	public async Task SessionPagingPreservesUserOrderAndTotal(int page, int itemCount, string? firstId)
	{
		var repository = DispatchProxy.Create<ISessionRepository, SessionRepositoryStub>();
		var stub = (SessionRepositoryStub)repository;
		stub.Sessions = new[] { "newest", "middle", "oldest" }
			.Select(id => new SessionData
			{
				Id = id,
				UserId = 7,
				UserAgent = "test",
			})
			.ToList();
		var getSessionsPageHandler = new GetSessionsPageHandler(repository);

		var result = await getSessionsPageHandler.HandleAsync(
			new GetSessionsPageQuery(7, new PageOptions { Page = page, PageSize = 2 }, "newest"),
			default
		);

		Assert.Equal(7, Assert.Single(stub.RequestedUsers));
		Assert.All(result.Items, session => Assert.Equal(session.Id == "newest", session.IsCurrent));
		Assert.Equal(3, result.TotalCount);
		Assert.Equal(2, result.TotalPages);
		Assert.Equal(itemCount, result.Items.Count);
		if (itemCount > 0)
			Assert.Equal(firstId, result.Items[0].Id);
	}

	[Fact]
	public async Task EmptySessionsAndCancellationDoNotCauseExtraRepositoryCalls()
	{
		var repository = DispatchProxy.Create<ISessionRepository, SessionRepositoryStub>();
		var stub = (SessionRepositoryStub)repository;
		var getSessionsPageHandler = new GetSessionsPageHandler(repository);
		var result = await getSessionsPageHandler.HandleAsync(new GetSessionsPageQuery(7, new PageOptions()), default);
		Assert.Empty(result.Items);
		Assert.Equal(0, result.TotalCount);
		Assert.Single(stub.RequestedUsers);
		stub.RequestedUsers.Clear();
		using var cancellation = new CancellationTokenSource();
		cancellation.Cancel();
		await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
			getSessionsPageHandler.HandleAsync(new GetSessionsPageQuery(7, new PageOptions()), cancellation.Token)
		);
		Assert.Empty(stub.RequestedUsers);
	}

	public class SessionRepositoryStub : DispatchProxy
	{
		public IReadOnlyList<SessionData> Sessions { get; set; } = [];
		public List<int> RequestedUsers { get; } = [];

		protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
		{
			if (targetMethod!.Name != nameof(ISessionRepository.ListByUserAsync))
				throw new InvalidOperationException($"Unexpected session operation: {targetMethod.Name}");

			RequestedUsers.Add((int)args![0]!);
			return Task.FromResult(Sessions);
		}
	}
}
