#r "nuget: Lestaly, 0.65.0"
#nullable enable
using Lestaly;
using Lestaly.Cx;

await Paved.RunAsync(config: c => c.AnyPause(), action: async () =>
{
    WriteLine("Restart service (with bind-mount) ...");
    var composeFile = ThisSource.RelativeFile("./docker/compose.yml");
    var bindFile = ThisSource.RelativeFile("./docker/volume-bind.yml");
    await "docker".args("compose", "--file", composeFile.FullName, "down", "--remove-orphans").result().success();
    await "docker".args("compose", "--file", composeFile.FullName, "--file", bindFile.FullName, "up", "-d", "--wait").result().success();

    WriteLine();
    WriteLine("Container up completed.");
    var pubPort = await "docker".args("compose", "--file", composeFile.FullName, "port", "app", "80").silent().result().success().output();
    var portNum = pubPort.AsSpan().SkipToken(':').TryParseNumber<ushort>();
    var serviceUrl = $"http://localhost:{portNum}";
    WriteLine("Service URL");
    WriteLine($" {Poster.Link[serviceUrl]}");
    WriteLine();
});
