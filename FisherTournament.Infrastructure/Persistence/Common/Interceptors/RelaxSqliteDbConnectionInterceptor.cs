using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace FisherTournament.Infrastructure.Persistence.Common.Interceptors;

public class RelaxSqliteDbConnectionInterceptor : IDbConnectionInterceptor
{
    private const string Command = "PRAGMA journal_mode = WAL; PRAGMA synchronous = NORMAL;";

    public void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
    {
        using var command = connection.CreateCommand();

        command.CommandText = Command;

        command.ExecuteNonQuery();
    }

    public async Task ConnectionOpenedAsync(DbConnection connection, ConnectionEndEventData eventData, CancellationToken cancellationToken = default)
    {
        using var command = connection.CreateCommand();

        command.CommandText = Command;

        await command.ExecuteNonQueryAsync();
    }
}