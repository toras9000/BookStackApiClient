using System.Runtime.CompilerServices;
using Lestaly;
using Microsoft.Extensions.DependencyInjection;

namespace BookStackApiClient.Tests;

public class BookStackClientTestsBase
{
    public Uri ApiBaseUri { get; } = new Uri(@"http://localhost:9988/api/");
    public string ApiTokenId { get; } = "00001111222233334444555566667777";
    public string ApiTokenSecret { get; } = "88889999aaaabbbbccccddddeeeeffff";
    public string ApiUser { get; } = "Admin";

    public DirectoryInfo AssetsDirectory { get; }
    public long ApiUserID => this.apiUserId.Value;
    public IServiceProvider ServiceProvider { get; }
    public IHttpClientFactory ClientFactory { get; }
    public HttpClient Client => this.ClientFactory.CreateClient();

    public BookStackClientTestsBase()
    {
        var thisAsm = System.Reflection.Assembly.GetExecutingAssembly();
        var asmDir = Path.GetDirectoryName(thisAsm.Location)?.AsDirectoryInfo() ?? throw new Exception();
        this.AssetsDirectory = asmDir.RelativeDirectory("assets");

        this.ServiceProvider = new ServiceCollection().AddHttpClient().BuildServiceProvider();
        this.ClientFactory = this.ServiceProvider.GetRequiredService<IHttpClientFactory>();

        this.apiUserId = new Lazy<long>(getApiUserId);
    }

    public string TestName([CallerMemberName] string member = "") => member;

    public string TestResPath(string relative) => this.TestResFile(relative).FullName;
    public FileInfo TestResFile(string relative) => this.AssetsDirectory.RelativeFile(relative);
    public Task<byte[]> TestResContentAsync(string relative) => this.TestResFile(relative).ReadAllBytesAsync();

    public async Task<long> GetApiUserIdAsync()
    {
        await using var adapter = new TestBackendAdapter();
        var id = await adapter.GetUserIdFromApiToken(this.ApiTokenId) ?? throw new Exception("Missing user");
        return id;
    }

    private Lazy<long> apiUserId;

    private long getApiUserId()
        => Task.Run(GetApiUserIdAsync).GetAwaiter().GetResult();
}
