#r "nuget: Lestaly.General, 0.100.0"
#nullable enable
using Lestaly;
using Lestaly.Cx;

await Paved.ProceedAsync(async () =>
{
    WriteLine("Stop service ...");
    var composeFile = ThisSource.RelativeFile("./docker/compose.yml");
    var bindFile = ThisSource.RelativeFile("./docker/volume-bind.yml");
    await "docker".args("compose", "--file", composeFile, "--file", bindFile, "down", "--remove-orphans");

    WriteLine("Delete volumes ...");
    ThisSource.RelativeDirectory("./docker/volumes").DeleteRecurse();

    WriteLine("Completed.");
});
