using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;
using MySqlConnector;

namespace ViaTrade.Infrastructure.DataBase.Interceptors;

public class MySqlExceptionTranslationInterceptor : DbCommandInterceptor
{
	public override Task CommandFailedAsync(
		DbCommand command,
		CommandErrorEventData eventData,
		CancellationToken cancellationToken = default
	)
	{
		if (ShouldTranslate())
			TranslateAndThrow(eventData.Exception);

		return base.CommandFailedAsync(command, eventData, cancellationToken);
	}

	public override void CommandFailed(DbCommand command, CommandErrorEventData eventData)
	{
		if (ShouldTranslate())
			TranslateAndThrow(eventData.Exception);

		base.CommandFailed(command, eventData);
	}

	private static bool ShouldTranslate()
	{
		return !SuppressExceptionTranslationScope.IsSuppressed;
	}

	private static void TranslateAndThrow(Exception exception)
	{
		var mySqlException = FindMySqlException(exception);
		if (mySqlException != null)
		{
			var translated = DatabaseMySqlExceptionTranslator.Translate(mySqlException);
			if (translated != null)
				throw translated;
		}
	}

	private static MySqlException? FindMySqlException(Exception exception)
	{
		for (Exception? current = exception; current != null; current = current.InnerException)
		{
			if (current is MySqlException mySqlEx)
				return mySqlEx;
		}

		return null;
	}
}
