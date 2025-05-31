#r "nuget: Lestaly, 0.83.0"
#nullable enable
using Lestaly;
using Lestaly.Cx;

await Paved.ProceedAsync(async () =>
{
    WriteLine("Restart service ...");
    var composeFile = ThisSource.RelativeFile("./docker/compose.yml");
    await "docker".args("compose", "--file", composeFile, "down", "--remove-orphans").result().success();
    await "docker".args("compose", "--file", composeFile, "up", "-d", "--wait").result().success();

    WriteLine();
    WriteLine("Container up completed.");
    var pubPort = await "docker".args("compose", "--file", composeFile, "port", "app", "80").silent().result().success().output(trim: true);
    var portNum = pubPort.AsSpan().SkipFirstToken(':').TryParseNumber<ushort>();
    var serviceUrl = $"http://localhost:{portNum}";
    WriteLine("Service URL");
    WriteLine($" {Poster.Link[serviceUrl]}");
    WriteLine();
});
