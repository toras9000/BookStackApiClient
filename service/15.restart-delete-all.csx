
#r "nuget: Lestaly, 0.50.0"
using System.Net.Http;
using System.Threading;
using Lestaly;
using Lestaly.Cx;

// Restart docker container with deletion of persistent data.
// (If it is not activated, it is simply activated.)

await Paved.RunAsync(async () =>
{
    var composeFile = ThisSource.RelativeFile("./docker/docker-compose.yml");

    Console.WriteLine("Stop service");
    await "docker".args("compose", "--file", composeFile.FullName, "down").result().success();

    Console.WriteLine("Delete config/repos");
    var volumeDir = ThisSource.RelativeDirectory("./docker/volumes");
    if (volumeDir.Exists) { volumeDir.DoFiles(c => c.File?.SetReadOnly(false)); volumeDir.Delete(recursive: true); }

    Console.WriteLine("Start service");
    await "docker".args("compose", "--file", composeFile.FullName, "up", "-d").result().success();

    Console.WriteLine("Waiting for accessible ...");
    using var checker = new HttpClient();
    while (!await checker.IsSuccessStatusAsync(new Uri("http://localhost:9988"))) await Task.Delay(1000);

    Console.WriteLine("completed.");
});
