using Microsoft.EntityFrameworkCore;
using MySqlConnector;

namespace Infrastructure.DataBase;

internal static class EfDatabaseOperation
{
	private readonly struct Unit;

	public static Task<T> ExecuteAsync<T>(Func<Task<T>> operation)
	{
		return ExecuteCoreAsync(operation);
	}

	public static Task ExecuteAsync(Func<Task> operation)
	{
		return ExecuteCoreAsync(async () =>
		{
			await operation();

			return default(Unit);
		});
	}

	private static async Task<T> ExecuteCoreAsync<T>(Func<Task<T>> operation)
	{
		try
		{
			return await operation();
		}
		catch (DbUpdateException exception)
		{
			var mySqlException = FindMySqlException(exception);

			if (mySqlException is null)
				throw;

			var translatedException = DatabaseMySqlExceptionTranslator.Translate(mySqlException);

			if (translatedException is null)
				throw;

			throw translatedException;
		}
		catch (MySqlException exception)
		{
			var translatedException = DatabaseMySqlExceptionTranslator.Translate(exception);

			if (translatedException is null)
				throw;

			throw translatedException;
		}
	}

	private static MySqlException? FindMySqlException(Exception exception)
	{
		for (
			Exception? currentException = exception;
			currentException is not null;
			currentException = currentException.InnerException
		)
		{
			if (currentException is MySqlException mySqlException)
				return mySqlException;
		}

		return null;
	}
}
