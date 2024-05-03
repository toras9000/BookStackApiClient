#r "nuget: Lestaly, 0.58.0"
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
    WriteLine("Service URL");
    ConsoleWig.Write(" ").WriteLink("http://localhost:9988").NewLine();
    WriteLine();
});
