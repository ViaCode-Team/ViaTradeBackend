using Microsoft.EntityFrameworkCore;
using MySqlConnector;

namespace Infrastructure.DataBase;

internal static class EfDatabaseOperation
{
	public static async Task<T> ExecuteAsync<T>(Func<Task<T>> operation)
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

			var translatedException = DatabaseExceptionTranslator.Translate(mySqlException);

			if (translatedException is null)
				throw;

			throw translatedException;
		}
		catch (MySqlException exception)
		{
			var translatedException = DatabaseExceptionTranslator.Translate(exception);

			if (translatedException is null)
				throw;

			throw translatedException;
		}
	}

	public static async Task ExecuteAsync(Func<Task> operation)
	{
		try
		{
			await operation();
		}
		catch (DbUpdateException exception)
		{
			var mySqlException = FindMySqlException(exception);

			if (mySqlException is null)
				throw;

			var translatedException = DatabaseExceptionTranslator.Translate(mySqlException);

			if (translatedException is null)
				throw;

			throw translatedException;
		}
		catch (MySqlException exception)
		{
			var translatedException = DatabaseExceptionTranslator.Translate(exception);

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
