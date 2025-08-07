#r "nuget: MySqlConnector, 2.4.0"
#r "nuget: Dapper, 2.1.66"
#r "nuget: BCrypt.Net-Next, 4.0.3"
#r "nuget: Lestaly.General, 0.102.0"
#nullable enable
using Dapper;
using Lestaly;
using Lestaly.Cx;
using MySqlConnector;

return await Paved.ProceedAsync(noPause: Args.RoughContains("--no-pause"), async () =>
{
    WriteLine("Detect database port");
    var composeFile = ThisSource.RelativeFile("./docker/compose.yml");
    var pubPort = await "docker".args("compose", "--file", composeFile, "port", "db", "3306").silent().result().success().output(trim: true);
    var portNum = pubPort.AsSpan().SkipToken(':').TryParseNumber<ushort>() ?? throw new PavedMessageException("Cannot get port number");

    WriteLine("Prepare db connection ...");
    var config = new MySqlConnectionStringBuilder();
    config.Server = "localhost";
    config.Port = portNum;
    config.UserID = "bookstack_user";
    config.Password = "bookstack_pass";
    config.Database = "bookstack_store";

    using var mysql = new MySqlConnection(config.ConnectionString);
    await mysql.OpenAsync();

    var tokenUser = "Admin";
    var tokenName = "TestToken";
    var tokenId = "00001111222233334444555566667777";
    var tokenSecret = "88889999aaaabbbbccccddddeeeeffff";

    WriteLine("Get user id ...");
    var tokenUserId = await mysql.ExecuteScalarAsync<uint?>(
        sql: "select id from users where name = @user",
        param: new { user = tokenUser }
    ) ?? throw new Exception($"User '{tokenUser}' not found");
    WriteLine($".. User: {tokenUser} [{tokenUserId}]");

    WriteLine("Check token id ...");
    var existingTokens = await mysql.QueryAsync(
        sql: "select user_id, token_id from api_tokens where user_id = @user_id or token_id = @token_id",
        param: new { user_id = tokenUserId, token_id = tokenId, },
        map: (uint user_id, string token_id) => new { user_id, token_id, },
        splitOn: "*"
    );
    if (existingTokens.Any(t => t.token_id == tokenId))
    {
        var satisfy = existingTokens.Any(t => t.token_id == tokenId && t.user_id == tokenUserId);
        if (!satisfy) throw new Exception("Duplicate token ID for other user");
        WriteLine(".. Already exists");
        return;
    }
    WriteLine(".. Not exists");

    WriteLine("Create api token ...");
    var hashSalt = BCrypt.Net.BCrypt.GenerateSalt(12, 'y');
    var secretHash = BCrypt.Net.BCrypt.HashPassword(tokenSecret, hashSalt);
    var tokenParam = new
    {
        name = tokenName,
        user_id = tokenUserId,
        token_id = tokenId,
        secret = secretHash,
        expires_at = DateTime.Now.AddYears(100),
    };
    await mysql.ExecuteAsync(
        sql: "insert into api_tokens (name, user_id, token_id, secret, expires_at) values (@name, @user_id, @token_id, @secret, @expires_at)",
        param: tokenParam
    );
    WriteLine(".. Token created");
});
