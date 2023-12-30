using Dapper;
using MySqlConnector;

namespace BookStackApiClient.Tests.helper;

public class TestBackendAdapter : IAsyncDisposable
{
    public static string DatabaseHost { get; } = "localhost";
    public static ushort DatabasePort { get; } = 9987;
    public static string DatabaseUser { get; } = "bookstack_user";
    public static string DatabasePass { get; } = "bookstack_pass";
    public static string DatabaseName { get; } = "bookstack_store";

    public async ValueTask<int?> GetUserIdFromApiToken(string token)
    {
        var db = await ensureConnectionAsync().ConfigureAwait(false);

        var parameters = new { token, };
        var query = $"""
        select
          api_tokens.user_id
        from api_tokens
        where api_tokens.token_id = @{nameof(parameters.token)}
        """;
        var id = await db.QueryFirstAsync<int?>(query, parameters).ConfigureAwait(false);

        return id;
    }

    public async ValueTask DisposeAsync()
    {
        if (this.connection == null) return;
        try { await this.connection.DisposeAsync(); }
        catch { }
        finally { this.connection = null!; }
    }

    private MySqlConnection? connection;

    private async ValueTask<MySqlConnection> ensureConnectionAsync()
    {
        if (this.connection != null) return this.connection;

        var builder = new MySqlConnectionStringBuilder();
        builder.Server = DatabaseHost;
        builder.Port = DatabasePort;
        builder.UserID = DatabaseUser;
        builder.Password = DatabasePass;
        builder.Database = DatabaseName;

        var conn = new MySqlConnection(builder.ConnectionString);
        await conn.OpenAsync().ConfigureAwait(false);

        this.connection = conn;

        return conn;
    }

}
