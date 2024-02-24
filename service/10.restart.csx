#r "nuget: Lestaly, 0.56.0"
using Lestaly;
using Lestaly.Cx;

// Restart docker container with deletion of persistent data.
// (If it is not activated, it is simply activated.)

await Paved.RunAsync(async () =>
{
    Console.WriteLine("Restart service ...");
    var composeFile = ThisSource.RelativeFile("./docker/docker-compose.yml");
    await "docker".args("compose", "--file", composeFile.FullName, "down").result().success();
    await "docker".args("compose", "--file", composeFile.FullName, "up", "-d", "--wait").result().success();

    Console.WriteLine("Open service URL.");
    await CmdShell.ExecAsync("http://localhost:9988");

    Console.WriteLine("completed.");
});
