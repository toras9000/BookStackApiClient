using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;

namespace BookStackApiClient.Tests;

public class BookStackClientTestsBase
{
    public Uri ApiBaseUri { get; } = new Uri(@"http://localhost:9988/api/");
    public string ApiTokenId { get; } = "00001111222233334444555566667777";
    public string ApiTokenSecret { get; } = "88889999aaaabbbbccccddddeeeeffff";
    public string ApiUser { get; } = "Admin";

    public DirectoryInfo AssetsDirectory { get; }
    public long ApiUserID { get; }
    public IServiceProvider ServiceProvider { get; }
    public IHttpClientFactory ClientFactory { get; }
    public HttpClient Client => this.ClientFactory.CreateClient();

    public BookStackClientTestsBase()
    {
        var thisAsm = System.Reflection.Assembly.GetExecutingAssembly();
        var asmDir = Path.GetDirectoryName(thisAsm.Location);
        var assetsDir = Path.Combine(asmDir!, "assets");
        this.AssetsDirectory = new DirectoryInfo(assetsDir);

        this.ServiceProvider = new ServiceCollection().AddHttpClient().BuildServiceProvider();
        this.ClientFactory = this.ServiceProvider.GetRequiredService<IHttpClientFactory>();
        this.ApiUserID = Task.Run(getApiUserIdAsync).GetAwaiter().GetResult();
    }

    protected string testResPath(string relative) => Path.Combine(this.AssetsDirectory.FullName, relative);
    protected Task<byte[]> testResContentAsync(string relative) => File.ReadAllBytesAsync(testResPath(relative));
    protected string testName(string suffix, [CallerMemberName] string member = "") => string.IsNullOrEmpty(suffix) ? member : $"{member}_{suffix}";

    protected async Task<long> getApiUserIdAsync()
    {
        await using var adapter = new TestBackendAdapter();
        var id = await adapter.GetUserIdFromApiToken(this.ApiTokenId);
        return id.Value;
    }
}
