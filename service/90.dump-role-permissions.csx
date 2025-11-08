#r "nuget: MySqlConnector, 2.4.0"
#r "nuget: Dapper, 2.1.66"
#r "nuget: Humanizer.Core, 2.14.1"
#r "nuget: Kokuban, 0.2.0"
#r "nuget: Lestaly.General, 0.108.0"
#nullable enable
using Dapper;
using Lestaly;
using Lestaly.Cx;
using Humanizer;
using Kokuban;
using MySqlConnector;

await Paved.ProceedAsync(async () =>
{
    var composeFile = ThisSource.RelativeFile("./docker/compose.yml");

    WriteLine("Detect database port");
    var pubPort = await "docker".args("compose", "--file", composeFile, "port", "db", "3306").silent().result().success().output(trim: true);
    var portNum = pubPort.AsSpan().SkipToken(':').TryParseNumber<ushort>() ?? throw new PavedMessageException("Cannot get port number");

    WriteLine("Open database");
    var connector = new MySqlConnectionStringBuilder();
    connector.Server = "localhost";
    connector.Port = portNum;
    connector.UserID = "bookstack_user";
    connector.Password = "bookstack_pass";
    connector.Database = "bookstack_store";

    using var db = new MySqlConnection(connector.ConnectionString);
    await db.OpenAsync();

    WriteLine("Query the definition of permissions.");
    var permissions = await db.QueryAsync(
        sql: "select id, name from role_permissions order by id",
        map: (uint id, string name) => new { id, name, },
        splitOn: "*"
    );

    WriteLine("Generate source code");
    var defines = permissions
        .Select(perm =>
        {
            var identity = perm.name.Humanize().Pascalize();
            return $$"""
                public static string {{identity}} { get; } = "{{perm.name}}";
            """;
        })
        .JoinString(Environment.NewLine);

    var source = $$"""
    namespace BookStackApiClient;

    #pragma warning disable CS1591 // 公開されている型またはメンバーの XML コメントがありません

    /// <summary>
    /// ロール権限定数
    /// </summary>
    public static class RolePermissions
    {
    {{defines}}
    }
    """;

    WriteLine("Save the generated source code");
    var genEnc = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
    var genFile = ThisSource.RelativeFile("../src/BookStackConstants.cs");
    await genFile.WriteAllTextAsync(source.ReplaceLineEndings(), genEnc);

    WriteLine(Chalk.Green["Completed."]);
});
