#r "nuget: MySqlConnector, 2.4.0"
#r "nuget: Dapper, 2.1.66"
#r "nuget: BCrypt.Net-Next, 4.0.3"
#r "nuget: Lestaly.General, 0.100.0"
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

    WriteLine("Setup api token ...");
    var config = new MySqlConnectionStringBuilder();
    config.Server = "localhost";
    config.Port = portNum;
    config.UserID = "bookstack_user";
    config.Password = "bookstack_pass";
    config.Database = "bookstack_store";

    using var mysql = new MySqlConnection(config.ConnectionString);
    await mysql.OpenAsync();

    var tokenName = "TestToken";
    var tokenExists = await mysql.QueryFirstAsync<long>("select count(*) from api_tokens where name = @name", param: new { name = tokenName, });
    if (0 < tokenExists)
    {
        WriteLine(".. Already exists");
        return;
    }

    var tokenId = "00001111222233334444555566667777";
    var tokenSecret = "88889999aaaabbbbccccddddeeeeffff";

    var adminId = await mysql.QueryFirstAsync<long>(sql: "select id from users where name = 'Admin'");
    var hashSalt = BCrypt.Net.BCrypt.GenerateSalt(12, 'y');
    var secretHash = BCrypt.Net.BCrypt.HashPassword(tokenSecret, hashSalt);
    var tokenParam = new
    {
        name = tokenName,
        token_id = tokenId,
        secret = secretHash,
        user_id = adminId,
        expires_at = DateTime.Now.AddYears(100),
    };
    await mysql.ExecuteAsync(
        sql: "insert into api_tokens (name, token_id, secret, user_id, expires_at) values (@name, @token_id, @secret, @user_id, @expires_at)",
        param: tokenParam
    );
    WriteLine(".. Token added");
});
