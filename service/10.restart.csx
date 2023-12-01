#r "nuget: Lestaly, 0.51.0"
using System.Net.Http;
using Lestaly;
using Lestaly.Cx;

// Restart docker container with deletion of persistent data.
// (If it is not activated, it is simply activated.)

await Paved.RunAsync(async () =>
{
    Console.WriteLine("Restart service ...");
    var composeFile = ThisSource.RelativeFile("./docker/docker-compose.yml");
    await "docker".args("compose", "--file", composeFile.FullName, "down").result().success();
    await "docker".args("compose", "--file", composeFile.FullName, "up", "-d").result().success();

    Console.WriteLine("Waiting for accessible ...");
    using var checker = new HttpClient();
    while (!await checker.IsSuccessStatusAsync(new Uri("http://localhost:9988"))) await Task.Delay(1000);

    Console.WriteLine("completed.");
});
