#r "nuget: Lestaly, 0.45.0"
using System.Threading;
using Lestaly;

// Restart docker container with deletion of persistent data.
// (If it is not activated, it is simply activated.)

await Paved.RunAsync(async () =>
{
    var composeFile = ThisSource.RelativeFile("./docker/docker-compose.yml");
    if (!composeFile.Exists) throw new PavedMessageException("Not found compose file");

    Console.WriteLine("Stop service");
    var downResult = await CmdProc.RunAsync("docker", new[] { "compose", "--file", composeFile.FullName, "down", });
    if (downResult.ExitCode != 0) throw new PavedMessageException($"Failed to down. ExitCode={downResult.ExitCode}\n{downResult.Output}");

    Console.WriteLine("Start service");
    var upResult = await CmdProc.RunAsync("docker", new[] { "compose", "--file", composeFile.FullName, "up", "-d", });
    if (upResult.ExitCode != 0) throw new PavedMessageException($"Failed to up. ExitCode={upResult.ExitCode}\n{upResult.Output}");
    
    Console.WriteLine("completed.");
});
