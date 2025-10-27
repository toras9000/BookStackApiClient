#r "nuget: Lestaly.General, 0.108.0"
#nullable enable
using Lestaly;
using Lestaly.Cx;

return await Paved.ProceedAsync(noPause: Args.RoughContains("--no-pause"), async () =>
{
    WriteLine("Restart service ...");
    var composeFile = ThisSource.RelativeFile("./docker/compose.yml");
    await "docker".args("compose", "--file", composeFile, "down", "--remove-orphans").result().success();
    await "docker".args("compose", "--file", composeFile, "up", "--detach", "--wait").result().success();
});
