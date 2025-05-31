namespace BookStackApiClient.Tests;

[TestClass]
public class BookStackClientSystemTests : BookStackClientTestsBase
{
    #region system
    [TestMethod()]
    public async Task SystemAsync()
    {
        // init
        using var client = new BookStackClient(this.ApiBaseUri, this.ApiTokenId, this.ApiTokenSecret, () => this.Client);

        // test call & validate
        var info = await client.SystemAsync();
        info.version.Should().NotBeEmpty();
        info.instance_id.Should().NotBeEmpty();
        info.app_name.Should().NotBeEmpty();
        info.app_logo.Should().NotBeEmpty();
        info.base_url.Should().NotBeEmpty();

        var version = BookStackVersion.Parse(info.version);
    }
    #endregion
}
